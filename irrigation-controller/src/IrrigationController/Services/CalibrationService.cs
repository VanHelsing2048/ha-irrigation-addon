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

        if (request.MeasurementsMm.Any(value => value < 0))
        {
            return null;
        }

        var average = request.MeasurementsMm.Average();
        if (average <= 0)
        {
            return null;
        }

        var min = request.MeasurementsMm.Min();
        var max = request.MeasurementsMm.Max();
        var precipitationRate = average / request.Minutes * 60;
        var uniformity = min / average * 100;

        var result = new CalibrationResult
        {
            ZoneId = zoneId,
            Minutes = request.Minutes,
            MeasurementsMm = request.MeasurementsMm,
            AverageMm = Math.Round(average, 2),
            MinMm = Math.Round(min, 2),
            MaxMm = Math.Round(max, 2),
            PrecipitationRateMmH = Math.Round(precipitationRate, 2),
            DistributionUniformityPercent = Math.Round(uniformity, 1),
            Recommendation = BuildRecommendation(precipitationRate, uniformity)
        };

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

    private static string BuildRecommendation(double precipitationRate, double uniformity)
    {
        var uniformityText = uniformity switch
        {
            >= 80 => "uniformity is good",
            >= 65 => "uniformity is acceptable",
            _ => "uniformity is low; check sprinkler coverage before trusting automatic timing"
        };

        return $"Set precipitation_rate_mm_h to {precipitationRate:0.##}; {uniformityText}.";
    }
}
