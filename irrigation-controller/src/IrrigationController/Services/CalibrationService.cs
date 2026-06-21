using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class CalibrationService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly CycleRunner _runner;

    public CalibrationService(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        CycleRunner runner)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _runner = runner;
    }

    public async Task<CommandResult> StartAsync(string zoneId, int minutes, CancellationToken cancellationToken)
    {
        if (minutes is < 1 or > 60)
        {
            return new CommandResult(false, "Calibration minutes must be between 1 and 60.");
        }

        return await _runner.StartZoneAsync(zoneId, TimeSpan.FromMinutes(minutes), cancellationToken);
    }

    public async Task<CalibrationResult?> CompleteAsync(
        string zoneId,
        CalibrationCompleteRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        if (!config.Zones.ContainsKey(zoneId) || request.Minutes <= 0 || request.MeasurementsMm.Count == 0)
        {
            return null;
        }

        var result = CalibrationCalculator.Calculate(zoneId, request);
        if (result is null)
        {
            return null;
        }

        var state = await _stateStore.GetAsync(cancellationToken);
        state.Calibrations[zoneId] = result;
        state.Events.Insert(0, new IrrigationEvent
        {
            Type = "calibration",
            ZoneId = zoneId,
            AmountMm = result.PrecipitationRateMmH,
            Message = $"Calibration completed. Rate={result.PrecipitationRateMmH:0.##}mm/h, uniformity={result.DistributionUniformityPercent:0.#}%."
        });

        if (state.Events.Count > 200)
        {
            state.Events.RemoveRange(200, state.Events.Count - 200);
        }

        await _stateStore.SaveAsync(state, cancellationToken);
        return result;
    }

    public async Task<CommandResult> ApplyAsync(string zoneId, CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        if (!config.Zones.TryGetValue(zoneId, out var zone))
        {
            return new CommandResult(false, $"Unknown zone {zoneId}.");
        }

        var state = await _stateStore.GetAsync(cancellationToken);
        if (!state.Calibrations.TryGetValue(zoneId, out var calibration))
        {
            return new CommandResult(false, $"No calibration is available for zone {zoneId}.");
        }

        zone.PrecipitationRateMmH = calibration.PrecipitationRateMmH;
        await _configStore.SaveAsync(config, cancellationToken);

        state.Events.Insert(0, new IrrigationEvent
        {
            Type = "calibration_applied",
            ZoneId = zoneId,
            AmountMm = calibration.PrecipitationRateMmH,
            Message = $"Applied calibrated precipitation rate {calibration.PrecipitationRateMmH:0.##}mm/h to configuration."
        });

        if (state.Events.Count > 200)
        {
            state.Events.RemoveRange(200, state.Events.Count - 200);
        }

        await _stateStore.SaveAsync(state, cancellationToken);
        return new CommandResult(true, $"Applied {calibration.PrecipitationRateMmH:0.##}mm/h to {zone.Name}.");
    }

}
