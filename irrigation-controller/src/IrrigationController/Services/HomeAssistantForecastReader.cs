using System.Text.Json;

namespace IrrigationController.Services;

public static class HomeAssistantForecastReader
{
    public static IEnumerable<JsonElement> EnumerateForecastItems(JsonElement root, string entityId)
    {
        if (!TryGetForecastArray(root, entityId, out var forecast))
        {
            yield break;
        }

        foreach (var item in forecast.EnumerateArray())
        {
            yield return item;
        }
    }

    public static bool TryGetForecastArray(JsonElement root, string entityId, out JsonElement forecast)
    {
        if (TryGetEntityForecast(root, entityId, out forecast))
        {
            return true;
        }

        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("service_response", out var serviceResponse)
            && TryGetEntityForecast(serviceResponse, entityId, out forecast))
        {
            return true;
        }

        return TryFindFirstForecastArray(root, out forecast);
    }

    private static bool TryGetEntityForecast(JsonElement root, string entityId, out JsonElement forecast)
    {
        forecast = default;
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(entityId, out var entity)
            || entity.ValueKind != JsonValueKind.Object
            || !entity.TryGetProperty("forecast", out forecast)
            || forecast.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return true;
    }

    private static bool TryFindFirstForecastArray(JsonElement root, out JsonElement forecast)
    {
        forecast = default;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (property.NameEquals("forecast") && property.Value.ValueKind == JsonValueKind.Array)
            {
                forecast = property.Value;
                return true;
            }

            if (property.Value.ValueKind == JsonValueKind.Object
                && TryFindFirstForecastArray(property.Value, out forecast))
            {
                return true;
            }
        }

        return false;
    }
}
