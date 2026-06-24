using System.Text.Json;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class DecisionPlanService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly HomeAssistantClient _homeAssistant;

    public DecisionPlanService(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        HomeAssistantClient homeAssistant)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _homeAssistant = homeAssistant;
    }

    public async Task<DecisionPlan> GetAsync(CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var state = await _stateStore.GetAsync(cancellationToken);
        var forecasts = await ReadForecastsAsync(config, cancellationToken);
        var currentWeatherState = await _homeAssistant.GetStateAsync(config.Weather.Entity, cancellationToken) ?? "";
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);

        return new DecisionPlan
        {
            Today = BuildDay("today", "Oggi", today, config, state, forecasts, currentWeatherState),
            Tomorrow = BuildDay("tomorrow", "Domani", today.AddDays(1), config, state, forecasts, "")
        };
    }

    private static DecisionDay BuildDay(
        string key,
        string label,
        DateOnly date,
        IrrigationConfig config,
        IrrigationRuntimeState state,
        List<ForecastItem> forecasts,
        string currentWeatherState)
    {
        var dayForecasts = forecasts.Where(item => DateOnly.FromDateTime(item.When.LocalDateTime) == date).ToList();
        var rain = dayForecasts.Sum(item => item.PrecipitationMm);
        var probability = dayForecasts.Count == 0 ? 0 : dayForecasts.Max(item => item.RainProbability);
        var et0 = EstimateEt0(dayForecasts);
        var effectiveRain = Math.Round(rain * config.Weather.RainEfficiency, 2);
        var weather = DescribeWeather(dayForecasts, rain, probability, currentWeatherState);
        var shouldSkip = rain >= config.Weather.SkipIfExpectedRainMmAbove
            || probability >= config.Weather.SkipIfRainProbabilityAbove;
        var cycles = BuildCycles(date, config, state, et0, effectiveRain, shouldSkip);

        var decision = shouldSkip
            ? "Salta"
            : effectiveRain >= 0.5
                ? "Riduce"
                : cycles.Count > 0 ? "Irriga" : "In attesa";

        return new DecisionDay
        {
            Key = key,
            Label = label,
            Icon = weather.Icon,
            WeatherLabel = weather.Label,
            Decision = decision,
            DecisionClass = shouldSkip ? "danger" : effectiveRain >= 0.5 ? "warn" : cycles.Count > 0 ? "ok" : "warn",
            ExpectedRainMm = Math.Round(rain, 1),
            EffectiveRainMm = effectiveRain,
            RainProbability = probability,
            Et0Mm = Math.Round(et0, 1),
            Cycles = cycles,
            Events = key == "today" ? BuildTodayEvents(state) : []
        };
    }

    private static List<DecisionCycle> BuildCycles(
        DateOnly date,
        IrrigationConfig config,
        IrrigationRuntimeState state,
        double et0,
        double effectiveRain,
        bool shouldSkip)
    {
        var cycles = new List<DecisionCycle>();

        foreach (var (cycleId, cycle) in config.Cycles)
        {
            if (!cycle.Enabled || cycle.Mode != CycleMode.Automatic || cycle.Schedule is null)
            {
                continue;
            }

            if (!ScheduleCalculator.IsScheduledDate(cycle.Schedule, date))
            {
                continue;
            }

            foreach (var configuredTime in cycle.Schedule.Times)
            {
                if (!TimeOnly.TryParse(configuredTime, out _))
                {
                    continue;
                }

                cycles.Add(new DecisionCycle
                {
                    Id = cycleId,
                    Name = cycle.Name,
                    Time = configuredTime,
                    Icon = shouldSkip ? "SKIP" : "DROP",
                    Decision = shouldSkip ? "Saltato" : "Previsto",
                    DecisionClass = shouldSkip ? "danger" : "ok",
                    Zones = shouldSkip ? [] : BuildZones(config, state, cycle, et0, effectiveRain)
                });
            }
        }

        return cycles
            .OrderBy(cycle => cycle.Time)
            .ThenBy(cycle => cycle.Name)
            .ToList();
    }

    private static List<DecisionZone> BuildZones(
        IrrigationConfig config,
        IrrigationRuntimeState state,
        CycleConfig cycle,
        double et0,
        double effectiveRain)
    {
        var zones = new List<DecisionZone>();
        foreach (var step in cycle.Steps)
        {
            foreach (var zoneId in step.Zones)
            {
                if (!config.Zones.TryGetValue(zoneId, out var zone))
                {
                    continue;
                }

                state.WaterBalance.TryGetValue(zoneId, out var currentDeficit);
                var projectedDeficit = Math.Max(0, currentDeficit + et0 * zone.CropCoefficient - effectiveRain - zone.TargetDeficitMm);
                var minutes = projectedDeficit <= 0
                    ? 0
                    : projectedDeficit / Math.Max(0.1, zone.PrecipitationRateMmH) * 60;
                minutes = minutes <= 0 ? 0 : Math.Clamp(minutes, zone.MinMinutes, Math.Min(zone.MaxMinutes, config.Safety.MaxZoneMinutes));

                zones.Add(new DecisionZone
                {
                    Id = zoneId,
                    Name = zone.Name,
                    Icon = minutes <= 0 ? "OK" : "DROP",
                    Text = minutes <= 0 ? "ok" : $"{Math.Round(minutes)} min"
                });
            }
        }

        return zones;
    }

    private static List<DecisionEvent> BuildTodayEvents(IrrigationRuntimeState state)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        return state.Events
            .Where(item => DateOnly.FromDateTime(item.Timestamp.LocalDateTime) == today)
            .Take(6)
            .Select(item => new DecisionEvent
            {
                Time = item.Timestamp.ToLocalTime().ToString("HH:mm"),
                Icon = item.Type switch
                {
                    "irrigation" => "DROP",
                    "water_balance" => "BAL",
                    _ => "INFO"
                },
                Text = item.ZoneId is null ? item.Message : $"{item.ZoneId}: {item.Message}"
            })
            .ToList();
    }

    private async Task<List<ForecastItem>> ReadForecastsAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        using var document = await _homeAssistant.GetForecastsAsync(config.Weather.Entity, config.Weather.ForecastType, cancellationToken);
        if (document is null || !document.RootElement.TryGetProperty(config.Weather.Entity, out var entity)
            || !entity.TryGetProperty("forecast", out var forecast))
        {
            return [];
        }

        var items = new List<ForecastItem>();
        foreach (var item in forecast.EnumerateArray())
        {
            if (!TryGetDateTime(item, out var when))
            {
                continue;
            }

            items.Add(new ForecastItem(
                when,
                ReadString(item, "condition") ?? "",
                ReadDouble(item, "precipitation", "native_precipitation") ?? 0,
                (int)(ReadDouble(item, "precipitation_probability") ?? 0),
                ReadDouble(item, "temperature", "native_temperature"),
                ReadDouble(item, "humidity"),
                ReadDouble(item, "wind_speed", "native_wind_speed"),
                ReadDouble(item, "cloud_coverage")));
        }

        return items;
    }

    private static (string Icon, string Label) DescribeWeather(List<ForecastItem> forecasts, double rain, int probability, string currentWeatherState)
    {
        if (forecasts.Count == 0)
        {
            return string.IsNullOrWhiteSpace(currentWeatherState)
                ? ("INFO", "Previsione non disponibile")
                : DescribeCondition(currentWeatherState);
        }

        var condition = forecasts
            .GroupBy(item => item.Condition)
            .OrderByDescending(group => group.Count())
            .First().Key;

        return DescribeCondition(condition, rain, probability);
    }

    private static (string Icon, string Label) DescribeCondition(string condition, double rain = 0, int probability = 0)
    {
        if (condition.Contains("lightning", StringComparison.OrdinalIgnoreCase))
        {
            return ("STORM", "Temporale");
        }

        if (rain >= 8 || condition.Contains("pouring", StringComparison.OrdinalIgnoreCase))
        {
            return ("RAIN", "Pioggia");
        }

        if (probability >= 60 || condition.Contains("rain", StringComparison.OrdinalIgnoreCase))
        {
            return ("PARTLY", "Variabile");
        }

        if (condition.Contains("sunny", StringComparison.OrdinalIgnoreCase)
            || condition.Contains("clear", StringComparison.OrdinalIgnoreCase))
        {
            return ("SUN", "Sole");
        }

        if (condition.Contains("partly", StringComparison.OrdinalIgnoreCase))
        {
            return ("PARTLY", "Variabile");
        }

        if (condition.Contains("cloud", StringComparison.OrdinalIgnoreCase))
        {
            return ("CLOUD", "Nuvoloso");
        }

        if (condition.Contains("fog", StringComparison.OrdinalIgnoreCase))
        {
            return ("FOG", "Nebbia");
        }

        if (condition.Contains("snow", StringComparison.OrdinalIgnoreCase))
        {
            return ("SNOW", "Neve");
        }

        return ("INFO", "Meteo non disponibile");
    }

    private static double EstimateEt0(List<ForecastItem> forecasts)
    {
        if (forecasts.Count == 0)
        {
            return 3;
        }

        var values = forecasts.Select(item =>
        {
            var temperature = item.Temperature ?? 20;
            var humidity = item.Humidity ?? 60;
            var windSpeed = item.WindSpeed ?? 5;
            var cloudCoverage = item.CloudCoverage ?? 50;

            var tempFactor = Math.Clamp((temperature - 5) / 25, 0.1, 1.4);
            var humidityFactor = Math.Clamp((100 - humidity) / 70, 0.2, 1.2);
            var windFactor = Math.Clamp(1 + windSpeed / 40, 1, 1.6);
            var sunFactor = Math.Clamp((100 - cloudCoverage) / 100, 0.25, 1);

            return 5 * tempFactor * humidityFactor * windFactor * sunFactor;
        });

        return Math.Round(values.Average(), 2);
    }

    private static bool TryGetDateTime(JsonElement item, out DateTimeOffset value)
    {
        value = default;
        return item.TryGetProperty("datetime", out var property)
            && DateTimeOffset.TryParse(property.GetString(), out value);
    }

    private static string? ReadString(JsonElement item, string propertyName) =>
        item.TryGetProperty(propertyName, out var property) ? property.GetString() : null;

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

    private sealed record ForecastItem(
        DateTimeOffset When,
        string Condition,
        double PrecipitationMm,
        int RainProbability,
        double? Temperature,
        double? Humidity,
        double? WindSpeed,
        double? CloudCoverage);
}
