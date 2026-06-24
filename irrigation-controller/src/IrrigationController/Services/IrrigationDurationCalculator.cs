using IrrigationController.Models;

namespace IrrigationController.Services;

public static class IrrigationDurationCalculator
{
    public static TimeSpan EstimateAutomaticDuration(
        IrrigationConfig config,
        IrrigationRuntimeState state,
        string zoneId,
        ZoneConfig zone,
        WeatherAdjustment weather,
        DateOnly today)
    {
        state.WaterBalance.TryGetValue(zoneId, out var currentDeficit);
        var projectedDeficit = currentDeficit;

        if (state.LastWaterBalanceUpdateDate != today.ToString("yyyy-MM-dd"))
        {
            var cropEt = weather.Et0Mm * zone.CropCoefficient;
            projectedDeficit = Math.Max(0, projectedDeficit + cropEt - weather.EffectiveRainMm);
        }

        var irrigationDeficit = Math.Max(0, projectedDeficit - zone.TargetDeficitMm);
        if (irrigationDeficit <= 0)
        {
            return TimeSpan.Zero;
        }

        var minutes = irrigationDeficit / Math.Max(0.1, zone.PrecipitationRateMmH) * 60;
        minutes = Math.Clamp(minutes, zone.MinMinutes, Math.Min(zone.MaxMinutes, config.Safety.MaxZoneMinutes));
        return TimeSpan.FromMinutes(minutes);
    }
}
