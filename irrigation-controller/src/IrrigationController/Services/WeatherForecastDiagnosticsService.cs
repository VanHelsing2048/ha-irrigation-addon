using System.Text.Json;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class WeatherForecastDiagnosticsService
{
    private readonly HomeAssistantClient _homeAssistant;

    public WeatherForecastDiagnosticsService(HomeAssistantClient homeAssistant)
    {
        _homeAssistant = homeAssistant;
    }

    public async Task<WeatherForecastCheck> CheckAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        var entity = config.Weather.Entity;
        var state = await _homeAssistant.GetStateAsync(entity, cancellationToken) ?? "";
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        var tomorrow = today.AddDays(1);

        return new WeatherForecastCheck
        {
            Entity = entity,
            State = state,
            CheckedAt = DateTimeOffset.UtcNow,
            Types =
            [
                await CheckTypeAsync(entity, "hourly", tomorrow, cancellationToken),
                await CheckTypeAsync(entity, "daily", tomorrow, cancellationToken)
            ]
        };
    }

    private async Task<WeatherForecastTypeCheck> CheckTypeAsync(
        string entity,
        string type,
        DateOnly tomorrow,
        CancellationToken cancellationToken)
    {
        using var document = await _homeAssistant.GetForecastsAsync(entity, type, cancellationToken);
        if (document is null)
        {
            return new WeatherForecastTypeCheck
            {
                Type = type,
                Available = false,
                Message = "Servizio forecast non disponibile."
            };
        }

        var items = HomeAssistantForecastReader
            .EnumerateForecastItems(document.RootElement, entity)
            .Where(item => TryGetDateTime(item, out _))
            .Select(item =>
            {
                TryGetDateTime(item, out var when);
                return new ForecastProbeItem(
                    when,
                    ReadString(item, "condition") ?? "",
                    ReadDouble(item, "precipitation", "native_precipitation") ?? 0,
                    (int)(ReadDouble(item, "precipitation_probability") ?? 0));
            })
            .OrderBy(item => item.When)
            .ToList();

        var tomorrowItems = items
            .Where(item => DateOnly.FromDateTime(item.When.LocalDateTime) == tomorrow)
            .ToList();

        return new WeatherForecastTypeCheck
        {
            Type = type,
            Available = items.Count > 0,
            RecordCount = items.Count,
            FirstForecastAt = items.FirstOrDefault()?.When,
            LastForecastAt = items.LastOrDefault()?.When,
            TomorrowAvailable = tomorrowItems.Count > 0,
            TomorrowRecords = tomorrowItems.Count,
            TomorrowCondition = DescribeCondition(tomorrowItems),
            TomorrowRainMm = Math.Round(tomorrowItems.Sum(item => item.PrecipitationMm), 1),
            TomorrowMaxRainProbability = tomorrowItems.Count == 0 ? 0 : tomorrowItems.Max(item => item.RainProbability),
            Message = items.Count == 0
                ? "Risposta ricevuta, ma nessun record forecast leggibile."
                : $"Forecast {type} letto correttamente."
        };
    }

    private static string DescribeCondition(List<ForecastProbeItem> items)
    {
        if (items.Count == 0)
        {
            return "";
        }

        return items
            .GroupBy(item => item.Condition)
            .OrderByDescending(group => group.Count())
            .First().Key;
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

    private sealed record ForecastProbeItem(
        DateTimeOffset When,
        string Condition,
        double PrecipitationMm,
        int RainProbability);
}

public sealed class WeatherForecastCheck
{
    public string Entity { get; set; } = "";
    public string State { get; set; } = "";
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<WeatherForecastTypeCheck> Types { get; set; } = [];
}

public sealed class WeatherForecastTypeCheck
{
    public string Type { get; set; } = "";
    public bool Available { get; set; }
    public int RecordCount { get; set; }
    public DateTimeOffset? FirstForecastAt { get; set; }
    public DateTimeOffset? LastForecastAt { get; set; }
    public bool TomorrowAvailable { get; set; }
    public int TomorrowRecords { get; set; }
    public string TomorrowCondition { get; set; } = "";
    public double TomorrowRainMm { get; set; }
    public int TomorrowMaxRainProbability { get; set; }
    public string Message { get; set; } = "";
}
