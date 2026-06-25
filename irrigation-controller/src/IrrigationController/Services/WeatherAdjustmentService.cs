using System.Text.Json;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class WeatherAdjustmentService
{
    private readonly HomeAssistantClient _homeAssistant;
    private readonly ILogger<WeatherAdjustmentService> _logger;

    public WeatherAdjustmentService(HomeAssistantClient homeAssistant, ILogger<WeatherAdjustmentService> logger)
    {
        _homeAssistant = homeAssistant;
        _logger = logger;
    }

    public async Task<WeatherAdjustment> CalculateAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        var weather = config.Weather;
        var et0 = await ReadExternalEt0Async(weather, cancellationToken);
        var expectedRain = 0d;
        var maxProbability = 0;
        var records = 0;
        DateTimeOffset? firstForecastAt = null;
        DateTimeOffset? lastForecastAt = null;
        var message = "Forecast Home Assistant disponibile.";

        using var forecasts = await _homeAssistant.GetForecastsAsync(weather.Entity, weather.ForecastType, cancellationToken);
        if (forecasts is not null)
        {
            var now = DateTimeOffset.UtcNow;
            var until = now.AddHours(weather.RainLookaheadHours);
            foreach (var item in EnumerateForecastItems(forecasts.RootElement, weather.Entity))
            {
                if (!TryGetDateTime(item, out var forecastTime) || forecastTime < now || forecastTime > until)
                {
                    continue;
                }

                expectedRain += ReadDouble(item, "precipitation", "native_precipitation") ?? 0;
                maxProbability = Math.Max(maxProbability, (int)(ReadDouble(item, "precipitation_probability") ?? 0));
                records++;
                firstForecastAt = firstForecastAt is null || forecastTime < firstForecastAt ? forecastTime : firstForecastAt;
                lastForecastAt = lastForecastAt is null || forecastTime > lastForecastAt ? forecastTime : lastForecastAt;

                if (et0 is null)
                {
                    et0 = EstimateEt0(item);
                }
            }
        }
        else
        {
            message = "Forecast Home Assistant non disponibile: uso fallback ET0.";
        }

        var et0Value = Math.Max(0, et0 ?? 3);
        var effectiveRain = Math.Max(0, expectedRain * weather.RainEfficiency);
        var shouldSkip = expectedRain >= weather.SkipIfExpectedRainMmAbove
            || maxProbability >= weather.SkipIfRainProbabilityAbove;

        if (records == 0 && forecasts is not null)
        {
            message = "Forecast ricevuto ma senza record nella finestra configurata.";
        }

        return new WeatherAdjustment(
            et0Value,
            expectedRain,
            effectiveRain,
            maxProbability,
            shouldSkip,
            weather.Entity,
            weather.ForecastType,
            records,
            firstForecastAt,
            lastForecastAt,
            records > 0,
            message);
    }

    private async Task<double?> ReadExternalEt0Async(WeatherConfig weather, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(weather.ExternalEt0SensorEntity))
        {
            return null;
        }

        return await _homeAssistant.GetNumericStateAsync(weather.ExternalEt0SensorEntity, cancellationToken);
    }

    private static IEnumerable<JsonElement> EnumerateForecastItems(JsonElement root, string entityId)
        => HomeAssistantForecastReader.EnumerateForecastItems(root, entityId);

    private static bool TryGetDateTime(JsonElement item, out DateTimeOffset value)
    {
        value = default;
        if (!item.TryGetProperty("datetime", out var property))
        {
            return false;
        }

        return DateTimeOffset.TryParse(property.GetString(), out value);
    }

    private static double EstimateEt0(JsonElement forecast)
    {
        var temperature = ReadDouble(forecast, "temperature", "native_temperature") ?? 20;
        var humidity = ReadDouble(forecast, "humidity") ?? 60;
        var windSpeed = ReadDouble(forecast, "wind_speed", "native_wind_speed") ?? 5;
        var cloudCoverage = ReadDouble(forecast, "cloud_coverage") ?? 50;

        var tempFactor = Math.Clamp((temperature - 5) / 25, 0.1, 1.4);
        var humidityFactor = Math.Clamp((100 - humidity) / 70, 0.2, 1.2);
        var windFactor = Math.Clamp(1 + windSpeed / 40, 1, 1.6);
        var sunFactor = Math.Clamp((100 - cloudCoverage) / 100, 0.25, 1);

        return Math.Round(5 * tempFactor * humidityFactor * windFactor * sunFactor, 2);
    }

    private static double? ReadDouble(JsonElement item, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!item.TryGetProperty(propertyName, out var property))
            {
                continue;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var number))
            {
                return number;
            }

            if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), out number))
            {
                return number;
            }
        }

        return null;
    }
}
