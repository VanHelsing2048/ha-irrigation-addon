using System.Text.Json;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class MqttDiscoveryPublisher : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationOverviewService _overviewService;
    private readonly HomeAssistantClient _homeAssistant;
    private readonly ILogger<MqttDiscoveryPublisher> _logger;
    private bool _discoveryPublished;

    public MqttDiscoveryPublisher(
        IrrigationConfigStore configStore,
        IrrigationOverviewService overviewService,
        HomeAssistantClient homeAssistant,
        ILogger<MqttDiscoveryPublisher> logger)
    {
        _configStore = configStore;
        _overviewService = overviewService;
        _homeAssistant = homeAssistant;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromSeconds(30);

            try
            {
                var config = await _configStore.GetAsync(stoppingToken);
                var mqtt = config.MqttDiscovery;
                delay = TimeSpan.FromSeconds(Math.Clamp(mqtt.PublishIntervalSeconds, 5, 3600));

                if (mqtt.Enabled)
                {
                    if (!_discoveryPublished)
                    {
                        await PublishDiscoveryAsync(mqtt, stoppingToken);
                        _discoveryPublished = true;
                    }

                    await PublishAvailabilityAsync(mqtt, "online", stoppingToken);
                    await PublishStateAsync(mqtt, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to publish MQTT discovery/state through Home Assistant.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configStore.GetAsync(cancellationToken);
            if (config.MqttDiscovery.Enabled)
            {
                await PublishAvailabilityAsync(config.MqttDiscovery, "offline", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to publish MQTT offline availability.");
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task PublishDiscoveryAsync(MqttDiscoveryConfig mqtt, CancellationToken cancellationToken)
    {
        var entities = new[]
        {
            Sensor(mqtt, "active_cycle", "Ciclo attivo", "{{ value_json.active_cycle }}", null, "mdi:sprinkler"),
            Sensor(mqtt, "active_zone", "Zona attiva", "{{ value_json.active_zone }}", null, "mdi:valve"),
            Sensor(mqtt, "remaining_seconds", "Tempo residuo", "{{ value_json.remaining_seconds }}", "s", "mdi:timer-outline"),
            Sensor(mqtt, "next_cycle", "Prossimo ciclo", "{{ value_json.next_cycle }}", null, "mdi:calendar-clock"),
            Sensor(mqtt, "next_run", "Prossima esecuzione", "{{ value_json.next_run }}", null, "mdi:clock-outline"),
            Sensor(mqtt, "config_errors", "Errori configurazione", "{{ value_json.config_errors }}", null, "mdi:alert-circle-outline"),
            Sensor(mqtt, "config_warnings", "Avvisi configurazione", "{{ value_json.config_warnings }}", null, "mdi:alert-outline"),
            BinarySensor(mqtt, "running", "Irrigazione in corso", "{{ value_json.running }}")
        };

        foreach (var entity in entities)
        {
            var topic = $"{mqtt.DiscoveryPrefix}/{entity.Component}/irrigation_controller/{entity.ObjectId}/config";
            await _homeAssistant.PublishMqttAsync(topic, JsonSerializer.Serialize(entity.Payload, JsonOptions), true, cancellationToken);
        }
    }

    private async Task PublishStateAsync(MqttDiscoveryConfig mqtt, CancellationToken cancellationToken)
    {
        var overview = await _overviewService.GetOverviewAsync(cancellationToken);
        var next = overview.Cycles.FirstOrDefault(cycle => cycle.NextRun is not null);
        var remainingSeconds = overview.Runner.ExpectedEndAt is null
            ? 0
            : Math.Max(0, (int)(overview.Runner.ExpectedEndAt.Value - DateTimeOffset.UtcNow).TotalSeconds);

        var state = new ControllerMqttState
        {
            Status = overview.Runner.Status ?? "idle",
            Running = overview.Runner.IsRunning,
            ActiveCycle = overview.Runner.CycleName ?? "",
            ActiveZone = overview.Runner.ZoneName ?? "",
            RemainingSeconds = remainingSeconds,
            NextCycle = next?.Name ?? "",
            NextRun = next?.NextRunText ?? "",
            ConfigErrors = overview.Validation.Errors.Count,
            ConfigWarnings = overview.Validation.Warnings.Count
        };

        await _homeAssistant.PublishMqttAsync(
            $"{mqtt.BaseTopic}/state",
            JsonSerializer.Serialize(state, JsonOptions),
            true,
            cancellationToken);
    }

    private Task PublishAvailabilityAsync(MqttDiscoveryConfig mqtt, string payload, CancellationToken cancellationToken) =>
        _homeAssistant.PublishMqttAsync($"{mqtt.BaseTopic}/availability", payload, true, cancellationToken);

    private static DiscoveryEntity Sensor(
        MqttDiscoveryConfig mqtt,
        string objectId,
        string name,
        string valueTemplate,
        string? unit,
        string icon)
    {
        var payload = BasePayload(mqtt, objectId, name, valueTemplate);
        payload["icon"] = icon;
        if (!string.IsNullOrWhiteSpace(unit))
        {
            payload["unit_of_measurement"] = unit;
        }

        return new DiscoveryEntity("sensor", objectId, payload);
    }

    private static DiscoveryEntity BinarySensor(MqttDiscoveryConfig mqtt, string objectId, string name, string valueTemplate)
    {
        var payload = BasePayload(mqtt, objectId, name, valueTemplate);
        payload["payload_on"] = "True";
        payload["payload_off"] = "False";
        payload["device_class"] = "running";
        return new DiscoveryEntity("binary_sensor", objectId, payload);
    }

    private static Dictionary<string, object> BasePayload(
        MqttDiscoveryConfig mqtt,
        string objectId,
        string name,
        string valueTemplate) => new()
    {
        ["name"] = name,
        ["unique_id"] = $"irrigation_controller_{objectId}",
        ["object_id"] = $"irrigazione_{objectId}",
        ["state_topic"] = $"{mqtt.BaseTopic}/state",
        ["value_template"] = valueTemplate,
        ["availability_topic"] = $"{mqtt.BaseTopic}/availability",
        ["payload_available"] = "online",
        ["payload_not_available"] = "offline",
        ["device"] = new Dictionary<string, object>
        {
            ["identifiers"] = new[] { "irrigation_controller" },
            ["name"] = "Irrigation Controller",
            ["manufacturer"] = "Local",
            ["model"] = "Home Assistant Add-on"
        }
    };

    private sealed record DiscoveryEntity(string Component, string ObjectId, Dictionary<string, object> Payload);
}
