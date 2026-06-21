using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class CycleRunner
{
    private readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly IrrigationConfigValidator _validator;
    private readonly HomeAssistantClient _homeAssistant;
    private readonly WeatherAdjustmentService _weather;
    private readonly ILogger<CycleRunner> _logger;
    private CancellationTokenSource? _activeRun;

    public RunnerSnapshot Current { get; private set; } = new() { Status = "idle" };

    public CycleRunner(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        IrrigationConfigValidator validator,
        HomeAssistantClient homeAssistant,
        WeatherAdjustmentService weather,
        ILogger<CycleRunner> logger)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _validator = validator;
        _homeAssistant = homeAssistant;
        _weather = weather;
        _logger = logger;
    }

    public async Task<CommandResult> StartCycleAsync(string cycleId, TriggerSource source, CancellationToken cancellationToken)
    {
        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            return new CommandResult(false, "Another cycle is already running.");
        }

        _activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => RunCycleGuardedAsync(cycleId, source, _activeRun.Token), CancellationToken.None);
        return new CommandResult(true, $"Cycle {cycleId} started.");
    }

    public async Task<CommandResult> StartZoneAsync(string zoneId, TimeSpan duration, CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);
        if (!validation.IsValid)
        {
            return new CommandResult(false, $"Invalid configuration: {validation.Errors[0].Path} - {validation.Errors[0].Message}");
        }

        if (!config.Zones.TryGetValue(zoneId, out var zone))
        {
            return new CommandResult(false, $"Unknown zone {zoneId}.");
        }

        var cycle = new CycleConfig
        {
            Name = $"Manual zone {zone.Name}",
            Mode = CycleMode.Manual,
            Steps = [new CycleStepConfig { Zones = [zoneId], DurationMinutes = (int)Math.Ceiling(duration.TotalMinutes) }]
        };

        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            return new CommandResult(false, "Another cycle is already running.");
        }

        _activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => RunCycleGuardedAsync($"zone_{zoneId}", TriggerSource.Manual, _activeRun.Token, cycle), CancellationToken.None);
        return new CommandResult(true, $"Zone {zoneId} started.");
    }

    public async Task StopAsync(string reason)
    {
        _logger.LogInformation("Stopping irrigation: {Reason}", reason);
        if (_activeRun is not null)
        {
            await _activeRun.CancelAsync();
        }
    }

    public async Task StopZoneAsync(string zoneId, CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        if (config.Zones.TryGetValue(zoneId, out var zone))
        {
            await _homeAssistant.TurnOffAsync(zone.Entity, cancellationToken);
        }
    }

    private async Task RunCycleGuardedAsync(string cycleId, TriggerSource source, CancellationToken cancellationToken, CycleConfig? overrideCycle = null)
    {
        try
        {
            await RunCycleAsync(cycleId, source, cancellationToken, overrideCycle);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cycle {CycleId} cancelled.", cycleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cycle {CycleId} failed.", cycleId);
        }
        finally
        {
            try
            {
                var config = await _configStore.GetAsync(CancellationToken.None);
                if (config.Safety.StopAllKnownZonesOnError)
                {
                    await StopAllKnownZonesAsync(config, CancellationToken.None);
                }
            }
            finally
            {
                Current = new RunnerSnapshot { Status = "idle" };
                _activeRun?.Dispose();
                _activeRun = null;
                _runLock.Release();
            }
        }
    }

    private async Task RunCycleAsync(string cycleId, TriggerSource source, CancellationToken cancellationToken, CycleConfig? overrideCycle)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);
        if (!validation.IsValid)
        {
            _logger.LogError("Invalid irrigation configuration: {Path} - {Message}", validation.Errors[0].Path, validation.Errors[0].Message);
            return;
        }

        var cycle = overrideCycle ?? (config.Cycles.TryGetValue(cycleId, out var configuredCycle) ? configuredCycle : null);
        if (cycle is null)
        {
            _logger.LogWarning("Unknown cycle {CycleId}.", cycleId);
            return;
        }

        if (!cycle.Enabled && source == TriggerSource.Schedule)
        {
            return;
        }

        var adjustment = cycle.Mode == CycleMode.Automatic || !config.Safety.ManualRunsIgnoreWeather
            ? await _weather.CalculateAsync(config, cancellationToken)
            : new WeatherAdjustment(0, 0, 0, 0, false);

        if (source == TriggerSource.Schedule && adjustment.ShouldSkip)
        {
            _logger.LogInformation("Cycle {CycleId} skipped due to weather forecast.", cycleId);
            return;
        }

        Current = new RunnerSnapshot
        {
            IsRunning = true,
            CycleId = cycleId,
            CycleName = cycle.Name,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "running"
        };

        foreach (var step in cycle.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RunStepAsync(config, cycle, step, adjustment, cancellationToken);
        }

        if (source == TriggerSource.Schedule)
        {
            await MarkScheduledRunAsync(cycleId, cancellationToken);
        }
    }

    private async Task RunStepAsync(
        IrrigationConfig config,
        CycleConfig cycle,
        CycleStepConfig step,
        WeatherAdjustment adjustment,
        CancellationToken cancellationToken)
    {
        foreach (var zoneId in step.Zones)
        {
            if (!config.Zones.TryGetValue(zoneId, out var zone))
            {
                _logger.LogWarning("Unknown zone {ZoneId}.", zoneId);
                continue;
            }

            var duration = await ResolveDurationAsync(config, cycle, step, zoneId, zone, adjustment, cancellationToken);
            if (duration <= TimeSpan.Zero)
            {
                _logger.LogInformation("Skipping zone {ZoneId}; calculated duration is zero.", zoneId);
                continue;
            }

            Current.ZoneId = zoneId;
            Current.ZoneName = zone.Name;
            Current.ExpectedEndAt = DateTimeOffset.UtcNow.Add(duration);

            await _homeAssistant.TurnOnAsync(zone.Entity, cancellationToken);
            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            finally
            {
                await _homeAssistant.TurnOffAsync(zone.Entity, CancellationToken.None);
                await ApplyIrrigationToBalanceAsync(zoneId, zone, duration, CancellationToken.None);
            }
        }
    }

    private async Task<TimeSpan> ResolveDurationAsync(
        IrrigationConfig config,
        CycleConfig cycle,
        CycleStepConfig step,
        string zoneId,
        ZoneConfig zone,
        WeatherAdjustment adjustment,
        CancellationToken cancellationToken)
    {
        if (cycle.Mode == CycleMode.Manual)
        {
            var manualMinutes = step.DurationMinutes ?? zone.MinMinutes;
            return TimeSpan.FromMinutes(Math.Clamp(manualMinutes, 0, config.Safety.MaxZoneMinutes));
        }

        if (!string.IsNullOrWhiteSpace(zone.SoilMoistureEntity) && zone.SkipIfSoilMoistureAbove is not null)
        {
            var moisture = await _homeAssistant.GetNumericStateAsync(zone.SoilMoistureEntity, cancellationToken);
            if (moisture >= zone.SkipIfSoilMoistureAbove)
            {
                return TimeSpan.Zero;
            }
        }

        var state = await _stateStore.GetAsync(cancellationToken);
        state.WaterBalance.TryGetValue(zoneId, out var previousDeficit);

        var cropEt = adjustment.Et0Mm * zone.CropCoefficient;
        var newDeficit = Math.Max(0, previousDeficit + cropEt - adjustment.EffectiveRainMm - zone.TargetDeficitMm);
        state.WaterBalance[zoneId] = newDeficit;
        await _stateStore.SaveAsync(state, cancellationToken);

        var minutes = newDeficit / Math.Max(0.1, zone.PrecipitationRateMmH) * 60;
        if (minutes <= 0)
        {
            return TimeSpan.Zero;
        }

        minutes = Math.Clamp(minutes, zone.MinMinutes, Math.Min(zone.MaxMinutes, config.Safety.MaxZoneMinutes));
        return TimeSpan.FromMinutes(minutes);
    }

    private async Task ApplyIrrigationToBalanceAsync(string zoneId, ZoneConfig zone, TimeSpan duration, CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.WaterBalance.TryGetValue(zoneId, out var deficit);
        var appliedMm = zone.PrecipitationRateMmH * duration.TotalHours;
        state.WaterBalance[zoneId] = Math.Max(0, deficit - appliedMm);
        await _stateStore.SaveAsync(state, cancellationToken);
    }

    private async Task MarkScheduledRunAsync(string cycleId, CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.LastScheduledRuns[cycleId] = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm");
        await _stateStore.SaveAsync(state, cancellationToken);
    }

    private async Task StopAllKnownZonesAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        foreach (var zone in config.Zones.Values)
        {
            try
            {
                await _homeAssistant.TurnOffAsync(zone.Entity, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to turn off {EntityId}.", zone.Entity);
            }
        }
    }
}
