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
    private readonly WaterBalanceService _waterBalance;
    private readonly IrrigationSafetyService _safety;
    private readonly DiagnosticsService _diagnostics;
    private readonly ILogger<CycleRunner> _logger;
    private CancellationTokenSource? _activeRun;

    public RunnerSnapshot Current { get; private set; } = new() { Status = "idle" };

    public CycleRunner(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        IrrigationConfigValidator validator,
        HomeAssistantClient homeAssistant,
        WeatherAdjustmentService weather,
        WaterBalanceService waterBalance,
        IrrigationSafetyService safety,
        DiagnosticsService diagnostics,
        ILogger<CycleRunner> logger)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _validator = validator;
        _homeAssistant = homeAssistant;
        _weather = weather;
        _waterBalance = waterBalance;
        _safety = safety;
        _diagnostics = diagnostics;
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
            Steps = [new CycleStepConfig { Zones = [zoneId], DurationSeconds = (int)Math.Ceiling(duration.TotalSeconds) }]
        };

        if (!await _runLock.WaitAsync(0, cancellationToken))
        {
            return new CommandResult(false, "Another cycle is already running.");
        }

        _activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => RunCycleGuardedAsync($"zone_{zoneId}", TriggerSource.Manual, _activeRun.Token, cycle), CancellationToken.None);
        return new CommandResult(true, $"Zone {zoneId} started.");
    }

    public async Task<CommandResult> DryRunCycleAsync(string cycleId, CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);
        if (!validation.IsValid)
        {
            return new CommandResult(false, $"Invalid configuration: {validation.Errors[0].Path} - {validation.Errors[0].Message}");
        }

        if (!config.Cycles.TryGetValue(cycleId, out var cycle))
        {
            return new CommandResult(false, $"Unknown cycle {cycleId}.");
        }

        var usesWeather = cycle.Mode == CycleMode.Automatic || !config.Safety.ManualRunsIgnoreWeather;
        var adjustment = usesWeather
            ? await _weather.CalculateAsync(config, cancellationToken)
            : new WeatherAdjustment(0, 0, 0, 0, false);
        if (usesWeather)
        {
            await _diagnostics.RecordWeatherAsync(adjustment, cancellationToken);
        }

        await RecordCycleEventAsync(
            "dry_run_started",
            "Simulazione ciclo avviata: nessuna valvola verra comandata.",
            cycleId,
            null,
            cancellationToken);

        if (adjustment.ShouldSkip)
        {
            await RecordCycleEventAsync(
                "dry_run_cycle_skipped",
                $"La logica salterebbe il ciclo per meteo: pioggia={adjustment.ExpectedRainMm:0.0}mm, probabilita={adjustment.MaxRainProbability}%.",
                cycleId,
                null,
                cancellationToken);
            return new CommandResult(true, $"Dry-run {cycleId} completed: cycle would be skipped.");
        }

        foreach (var step in cycle.Steps)
        {
            foreach (var zoneId in step.Zones)
            {
                if (!config.Zones.TryGetValue(zoneId, out var zone))
                {
                    continue;
                }

                var duration = await ResolveDurationAsync(config, cycle, step, zoneId, zone, cancellationToken);
                if (duration <= TimeSpan.Zero)
                {
                    await RecordCycleEventAsync(
                        "dry_run_zone_skipped",
                        $"La logica salterebbe {zone.Name}: durata calcolata zero.",
                        cycleId,
                        zoneId,
                        cancellationToken);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(config.Hydraulic.MasterValveEntity))
                {
                    await RecordCycleEventAsync(
                        "dry_run_master_valve",
                        $"La valvola master verrebbe aperta prima di {zone.Name}.",
                        cycleId,
                        null,
                        cancellationToken);
                }

                await RecordCycleEventAsync(
                    "dry_run_zone_planned",
                    $"La logica irrigerebbe {zone.Name} per {FormatDuration(duration)}.",
                    cycleId,
                    zoneId,
                    cancellationToken);
            }
        }

        await RecordCycleEventAsync(
            "dry_run_completed",
            "Simulazione ciclo completata.",
            cycleId,
            null,
            cancellationToken);

        return new CommandResult(true, $"Dry-run {cycleId} completed. Check the cycle register.");
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
            await _safety.TurnOffZoneAsync(zone, config.Safety, cancellationToken);
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
            await _diagnostics.RecordErrorAsync("cycle_runner", ex.Message, CancellationToken.None);
        }
        finally
        {
            try
            {
                var config = await _configStore.GetAsync(CancellationToken.None);
                if (config.Safety.StopAllKnownZonesOnError)
                {
                    await _safety.StopAllKnownZonesAsync(config, CancellationToken.None);
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

        var usesWeather = cycle.Mode == CycleMode.Automatic || !config.Safety.ManualRunsIgnoreWeather;
        var adjustment = usesWeather
            ? await _weather.CalculateAsync(config, cancellationToken)
            : new WeatherAdjustment(0, 0, 0, 0, false);
        if (usesWeather)
        {
            await _diagnostics.RecordWeatherAsync(adjustment, cancellationToken);
        }

        if (source == TriggerSource.Schedule && adjustment.ShouldSkip)
        {
            _logger.LogInformation("Cycle {CycleId} skipped due to weather forecast.", cycleId);
            await RecordCycleEventAsync(
                "cycle_skipped",
                $"Ciclo saltato per meteo: pioggia={adjustment.ExpectedRainMm:0.0}mm, probabilita={adjustment.MaxRainProbability}%.",
                cycleId,
                null,
                cancellationToken);
            await _diagnostics.RecordDecisionAsync(
                "cycle_skipped",
                $"Skipped due to forecast: rain={adjustment.ExpectedRainMm:0.0}mm, probability={adjustment.MaxRainProbability}%.",
                cycleId,
                null,
                cancellationToken);
            return;
        }

        if (cycle.Mode == CycleMode.Automatic)
        {
            await _waterBalance.EnsureDailyBalanceAsync(config, adjustment, cancellationToken);
        }

        Current = new RunnerSnapshot
        {
            IsRunning = true,
            CycleId = cycleId,
            CycleName = cycle.Name,
            StartedAt = DateTimeOffset.UtcNow,
            Status = "running"
        };

        await RecordCycleEventAsync(
            "cycle_started",
            $"Ciclo avviato ({source}).",
            cycleId,
            null,
            cancellationToken);

        foreach (var step in cycle.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RunStepAsync(config, cycle, step, adjustment, cancellationToken);
        }

        if (source == TriggerSource.Schedule)
        {
            await MarkScheduledRunAsync(cycleId, cancellationToken);
        }

        await RecordCycleEventAsync(
            "cycle_completed",
            "Ciclo completato.",
            cycleId,
            null,
            cancellationToken);
    }

    private async Task RunStepAsync(
        IrrigationConfig config,
        CycleConfig cycle,
        CycleStepConfig step,
        WeatherAdjustment adjustment,
        CancellationToken cancellationToken)
    {
        var runs = new List<ZoneRun>();
        foreach (var zoneId in step.Zones)
        {
            if (!config.Zones.TryGetValue(zoneId, out var zone))
            {
                _logger.LogWarning("Unknown zone {ZoneId}.", zoneId);
                continue;
            }

            var duration = await ResolveDurationAsync(config, cycle, step, zoneId, zone, cancellationToken);
            if (duration <= TimeSpan.Zero)
            {
                _logger.LogInformation("Skipping zone {ZoneId}; calculated duration is zero.", zoneId);
                await _diagnostics.RecordDecisionAsync(
                    "zone_skipped",
                    "Calculated duration is zero.",
                    Current.CycleId,
                    zoneId,
                    cancellationToken);
                await RecordCycleEventAsync(
                    "zone_skipped",
                    $"Zona {zone.Name} saltata: durata calcolata zero.",
                    Current.CycleId,
                    zoneId,
                    cancellationToken);
                continue;
            }

            runs.Add(new ZoneRun(zoneId, zone, duration));
        }

        if (runs.Count == 0)
        {
            return;
        }

        if (!config.Hydraulic.AllowParallelZones || config.Hydraulic.MaxParallelZones <= 1)
        {
            foreach (var run in runs)
            {
                await RunWithMasterValveAsync(config, [run], async () =>
                {
                    await RunSingleZoneAsync(config, run, cancellationToken);
                    await PauseBetweenZonesAsync(config, cancellationToken);
                }, cancellationToken);
            }

            return;
        }

        foreach (var batch in runs.Chunk(config.Hydraulic.MaxParallelZones))
        {
            Current.ZoneId = string.Join(",", batch.Select(item => item.ZoneId));
            Current.ZoneName = string.Join(", ", batch.Select(item => item.Zone.Name));
            Current.ExpectedEndAt = DateTimeOffset.UtcNow.Add(batch.Max(item => item.Duration));
            await RunWithMasterValveAsync(config, batch, async () =>
            {
                await Task.WhenAll(batch.Select(run => RunSingleZoneAsync(config, run, cancellationToken)));
                await PauseBetweenZonesAsync(config, cancellationToken);
            }, cancellationToken);
        }
    }

    private async Task RunWithMasterValveAsync(
        IrrigationConfig config,
        IReadOnlyCollection<ZoneRun> runs,
        Func<Task> operation,
        CancellationToken cancellationToken)
    {
        await TurnOnMasterValveAsync(config, runs, cancellationToken);
        try
        {
            await operation();
        }
        finally
        {
            await TurnOffMasterValveAsync(config, runs, CancellationToken.None);
        }
    }

    private async Task RunSingleZoneAsync(IrrigationConfig config, ZoneRun run, CancellationToken cancellationToken)
    {
        if (!config.Hydraulic.AllowParallelZones)
        {
            Current.ZoneId = run.ZoneId;
            Current.ZoneName = run.Zone.Name;
            Current.ExpectedEndAt = DateTimeOffset.UtcNow.Add(run.Duration);
        }

        await _safety.TurnOnZoneAsync(run.Zone, config.Safety, cancellationToken);
        await RecordCycleEventAsync(
            "zone_started",
            $"Zona {run.Zone.Name} avviata per {FormatDuration(run.Duration)}.",
            Current.CycleId,
            run.ZoneId,
            cancellationToken);
        try
        {
            await Task.Delay(run.Duration, cancellationToken);
        }
        finally
        {
            await _safety.TurnOffZoneAsync(run.Zone, config.Safety, CancellationToken.None);
            await _waterBalance.ApplyIrrigationAsync(run.ZoneId, run.Zone, run.Duration, CancellationToken.None);
            await RecordCycleEventAsync(
                "zone_completed",
                $"Zona {run.Zone.Name} completata dopo {FormatDuration(run.Duration)}.",
                Current.CycleId,
                run.ZoneId,
                CancellationToken.None);
        }
    }

    private async Task TurnOnMasterValveAsync(IrrigationConfig config, IReadOnlyCollection<ZoneRun> runs, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Hydraulic.MasterValveEntity))
        {
            return;
        }

        await _safety.TurnOnZoneAsync(MasterValve(config), config.Safety, cancellationToken);
        await RecordCycleEventAsync(
            "master_valve_started",
            $"Valvola master aperta per {string.Join(", ", runs.Select(run => run.Zone.Name))}.",
            Current.CycleId,
            null,
            cancellationToken);
    }

    private async Task TurnOffMasterValveAsync(IrrigationConfig config, IReadOnlyCollection<ZoneRun> runs, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.Hydraulic.MasterValveEntity))
        {
            return;
        }

        await _safety.TurnOffZoneAsync(MasterValve(config), config.Safety, cancellationToken);
        await RecordCycleEventAsync(
            "master_valve_stopped",
            "Valvola master chiusa.",
            Current.CycleId,
            null,
            cancellationToken);
    }

    private static ZoneConfig MasterValve(IrrigationConfig config) => new()
    {
        Name = "Valvola master",
        Entity = config.Hydraulic.MasterValveEntity ?? ""
    };

    private Task RecordCycleEventAsync(
        string type,
        string message,
        string? cycleId,
        string? zoneId,
        CancellationToken cancellationToken) =>
        _diagnostics.RecordEventAsync(type, message, cycleId, zoneId, cancellationToken);

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");

    private static async Task PauseBetweenZonesAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        if (config.Hydraulic.PauseBetweenZonesSeconds > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(config.Hydraulic.PauseBetweenZonesSeconds), cancellationToken);
        }
    }

    private async Task<TimeSpan> ResolveDurationAsync(
        IrrigationConfig config,
        CycleConfig cycle,
        CycleStepConfig step,
        string zoneId,
        ZoneConfig zone,
        CancellationToken cancellationToken)
    {
        if (cycle.Mode == CycleMode.Manual)
        {
            var manualDuration = step.DurationSeconds is > 0
                ? TimeSpan.FromSeconds(step.DurationSeconds.Value)
                : TimeSpan.FromMinutes(step.DurationMinutes ?? zone.MinMinutes);
            var maxDuration = TimeSpan.FromMinutes(config.Safety.MaxZoneMinutes);
            return manualDuration > maxDuration ? maxDuration : manualDuration;
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

        var irrigationDeficit = Math.Max(0, previousDeficit - zone.TargetDeficitMm);
        var minutes = irrigationDeficit / Math.Max(0.1, zone.PrecipitationRateMmH) * 60;
        if (minutes <= 0)
        {
            return TimeSpan.Zero;
        }

        minutes = Math.Clamp(minutes, zone.MinMinutes, Math.Min(zone.MaxMinutes, config.Safety.MaxZoneMinutes));
        return TimeSpan.FromMinutes(minutes);
    }

    private async Task MarkScheduledRunAsync(string cycleId, CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.LastScheduledRuns[cycleId] = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm");
        await _stateStore.SaveAsync(state, cancellationToken);
    }

    private sealed record ZoneRun(string ZoneId, ZoneConfig Zone, TimeSpan Duration);
}
