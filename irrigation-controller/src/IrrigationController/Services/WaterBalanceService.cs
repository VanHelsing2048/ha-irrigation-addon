using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class WaterBalanceService
{
    private readonly IrrigationStateStore _stateStore;
    private readonly ILogger<WaterBalanceService> _logger;

    public WaterBalanceService(IrrigationStateStore stateStore, ILogger<WaterBalanceService> logger)
    {
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task EnsureDailyBalanceAsync(
        IrrigationConfig config,
        WeatherAdjustment weather,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime).ToString("yyyy-MM-dd");
        var state = await _stateStore.GetAsync(cancellationToken);

        if (state.LastWaterBalanceUpdateDate == today)
        {
            return;
        }

        foreach (var (zoneId, zone) in config.Zones)
        {
            state.WaterBalance.TryGetValue(zoneId, out var currentDeficit);
            var cropEt = weather.Et0Mm * zone.CropCoefficient;
            var updatedDeficit = Math.Max(0, currentDeficit + cropEt - weather.EffectiveRainMm);
            state.WaterBalance[zoneId] = Math.Round(updatedDeficit, 2);

            AddEvent(state, new IrrigationEvent
            {
                Type = "water_balance",
                ZoneId = zoneId,
                AmountMm = Math.Round(cropEt - weather.EffectiveRainMm, 2),
                Message = $"Daily balance updated. ETc={cropEt:0.0}mm, rain={weather.EffectiveRainMm:0.0}mm, deficit={updatedDeficit:0.0}mm."
            });
        }

        state.LastWaterBalanceUpdateDate = today;
        await _stateStore.SaveAsync(state, cancellationToken);
        _logger.LogInformation("Water balance updated for {Date}.", today);
    }

    public async Task ApplyIrrigationAsync(
        string zoneId,
        ZoneConfig zone,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
        var state = await _stateStore.GetAsync(cancellationToken);
        state.WaterBalance.TryGetValue(zoneId, out var deficit);
        var appliedMm = zone.PrecipitationRateMmH * duration.TotalHours;
        var updatedDeficit = Math.Max(0, deficit - appliedMm);
        state.WaterBalance[zoneId] = Math.Round(updatedDeficit, 2);

        AddEvent(state, new IrrigationEvent
        {
            Type = "irrigation",
            ZoneId = zoneId,
            AmountMm = Math.Round(appliedMm, 2),
            Message = $"Applied {appliedMm:0.0}mm in {duration.TotalMinutes:0.#} minutes."
        });

        await _stateStore.SaveAsync(state, cancellationToken);
    }

    private static void AddEvent(IrrigationRuntimeState state, IrrigationEvent irrigationEvent)
    {
        state.Events.Insert(0, irrigationEvent);
        if (state.Events.Count > 200)
        {
            state.Events.RemoveRange(200, state.Events.Count - 200);
        }
    }
}
