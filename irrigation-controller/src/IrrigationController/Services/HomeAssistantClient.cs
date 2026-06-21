using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IrrigationController.Services;

public sealed class HomeAssistantClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<HomeAssistantClient> _logger;

    public HomeAssistantClient(HttpClient httpClient, IConfiguration configuration, ILogger<HomeAssistantClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = configuration["HA_BASE_URL"]
            ?? Environment.GetEnvironmentVariable("HA_BASE_URL")
            ?? "http://supervisor/core/api";

        var token = configuration["HA_TOKEN"]
            ?? Environment.GetEnvironmentVariable("HA_TOKEN")
            ?? Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");

        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public Task TurnOnAsync(string entityId, CancellationToken cancellationToken) =>
        CallServiceAsync("switch", "turn_on", new { entity_id = entityId }, cancellationToken);

    public Task TurnOffAsync(string entityId, CancellationToken cancellationToken) =>
        CallServiceAsync("switch", "turn_off", new { entity_id = entityId }, cancellationToken);

    public async Task<double?> GetNumericStateAsync(string entityId, CancellationToken cancellationToken)
    {
        var state = await GetStateAsync(entityId, cancellationToken);
        if (state is null)
        {
            return null;
        }

        return double.TryParse(state, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public async Task<string?> GetStateAsync(string entityId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"states/{Uri.EscapeDataString(entityId)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Unable to read Home Assistant state for {EntityId}: {StatusCode}", entityId, response.StatusCode);
            return null;
        }

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("state", out var state)
            ? state.GetString()
            : null;
    }

    public async Task<JsonDocument?> GetForecastsAsync(string weatherEntity, string forecastType, CancellationToken cancellationToken)
    {
        var payload = new
        {
            entity_id = weatherEntity,
            type = forecastType
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("services/weather/get_forecasts?return_response", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Unable to fetch weather forecasts from {WeatherEntity}: {StatusCode}", weatherEntity, response.StatusCode);
            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    public Task PublishMqttAsync(string topic, string payload, bool retain, CancellationToken cancellationToken) =>
        CallServiceAsync("mqtt", "publish", new
        {
            topic,
            payload,
            retain
        }, cancellationToken);

    private async Task CallServiceAsync(string domain, string service, object payload, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync($"services/{domain}/{service}", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
