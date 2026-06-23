using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class IrrigationConfigValidator
{
    public ConfigValidationResult Validate(IrrigationConfig config)
    {
        var result = new ConfigValidationResult();

        ValidateWeather(config, result);
        ValidateMqttDiscovery(config, result);
        ValidateSafety(config, result);
        ValidateHydraulic(config, result);
        ValidateZones(config, result);
        ValidateCycles(config, result);

        return result;
    }

    private static void ValidateHydraulic(IrrigationConfig config, ConfigValidationResult result)
    {
        if (config.Hydraulic.MaxParallelZones is < 1 or > 16)
        {
            Error(result, "hydraulic.max_parallel_zones", "Max parallel zones must be between 1 and 16.");
        }

        if (!config.Hydraulic.AllowParallelZones && config.Hydraulic.MaxParallelZones > 1)
        {
            Warning(result, "hydraulic.max_parallel_zones", "Max parallel zones is ignored while parallel zones are disabled.");
        }

        if (config.Hydraulic.PauseBetweenZonesSeconds is < 0 or > 3600)
        {
            Error(result, "hydraulic.pause_between_zones_seconds", "Pause between zones must be between 0 and 3600 seconds.");
        }
    }

    private static void ValidateMqttDiscovery(IrrigationConfig config, ConfigValidationResult result)
    {
        if (!config.MqttDiscovery.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(config.MqttDiscovery.DiscoveryPrefix))
        {
            Error(result, "mqtt_discovery.discovery_prefix", "MQTT discovery prefix is required when discovery is enabled.");
        }

        if (string.IsNullOrWhiteSpace(config.MqttDiscovery.BaseTopic))
        {
            Error(result, "mqtt_discovery.base_topic", "MQTT base topic is required when discovery is enabled.");
        }

        if (config.MqttDiscovery.PublishIntervalSeconds is < 5 or > 3600)
        {
            Error(result, "mqtt_discovery.publish_interval_seconds", "Publish interval must be between 5 and 3600 seconds.");
        }
    }

    private static void ValidateWeather(IrrigationConfig config, ConfigValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(config.Weather.Entity))
        {
            Error(result, "weather.entity", "Weather entity is required.");
        }

        if (config.Weather.RainLookaheadHours is < 1 or > 168)
        {
            Error(result, "weather.rain_lookahead_hours", "Rain lookahead must be between 1 and 168 hours.");
        }

        if (config.Weather.RainEfficiency is < 0 or > 1)
        {
            Error(result, "weather.rain_efficiency", "Rain efficiency must be between 0 and 1.");
        }

        if (config.Weather.SkipIfRainProbabilityAbove is < 0 or > 100)
        {
            Error(result, "weather.skip_if_rain_probability_above", "Rain probability threshold must be between 0 and 100.");
        }
    }

    private static void ValidateSafety(IrrigationConfig config, ConfigValidationResult result)
    {
        if (config.Safety.MaxZoneMinutes is < 1 or > 240)
        {
            Error(result, "safety.max_zone_minutes", "Max zone minutes must be between 1 and 240.");
        }

        if (config.Safety.SwitchRetryCount is < 0 or > 10)
        {
            Error(result, "safety.switch_retry_count", "Switch retry count must be between 0 and 10.");
        }

        if (config.Safety.SwitchRetryDelayMs is < 100 or > 30000)
        {
            Error(result, "safety.switch_retry_delay_ms", "Switch retry delay must be between 100 and 30000 ms.");
        }
    }

    private static void ValidateZones(IrrigationConfig config, ConfigValidationResult result)
    {
        if (config.Zones.Count == 0)
        {
            Error(result, "zones", "At least one zone is required.");
            return;
        }

        foreach (var (zoneId, zone) in config.Zones)
        {
            var path = $"zones.{zoneId}";

            if (string.IsNullOrWhiteSpace(zone.Name))
            {
                Error(result, $"{path}.name", "Zone name is required.");
            }

            if (string.IsNullOrWhiteSpace(zone.Entity))
            {
                Error(result, $"{path}.entity", "Home Assistant switch or valve entity is required.");
            }
            else if (!zone.Entity.StartsWith("switch.", StringComparison.OrdinalIgnoreCase)
                && !zone.Entity.StartsWith("valve.", StringComparison.OrdinalIgnoreCase))
            {
                Warning(result, $"{path}.entity", "Expected a Home Assistant switch or valve entity.");
            }

            if (zone.PrecipitationRateMmH <= 0)
            {
                Error(result, $"{path}.precipitation_rate_mm_h", "Precipitation rate must be greater than zero.");
            }

            if (zone.CropCoefficient <= 0)
            {
                Error(result, $"{path}.crop_coefficient", "Crop coefficient must be greater than zero.");
            }

            if (zone.MinMinutes < 0)
            {
                Error(result, $"{path}.min_minutes", "Minimum minutes cannot be negative.");
            }

            if (zone.MaxMinutes <= 0)
            {
                Error(result, $"{path}.max_minutes", "Maximum minutes must be greater than zero.");
            }

            if (zone.MinMinutes > zone.MaxMinutes)
            {
                Error(result, path, "Minimum minutes cannot be greater than maximum minutes.");
            }
        }
    }

    private static void ValidateCycles(IrrigationConfig config, ConfigValidationResult result)
    {
        if (config.Cycles.Count == 0)
        {
            Warning(result, "cycles", "No cycles are configured.");
            return;
        }

        foreach (var (cycleId, cycle) in config.Cycles)
        {
            var path = $"cycles.{cycleId}";

            if (string.IsNullOrWhiteSpace(cycle.Name))
            {
                Error(result, $"{path}.name", "Cycle name is required.");
            }

            if (cycle.Steps.Count == 0)
            {
                Error(result, $"{path}.steps", "At least one step is required.");
            }

            if (cycle.Mode == CycleMode.Automatic)
            {
                ValidateAutomaticSchedule(cycle, result, path);
            }

            for (var index = 0; index < cycle.Steps.Count; index++)
            {
                ValidateStep(config, cycle, result, path, index);
            }
        }
    }

    private static void ValidateAutomaticSchedule(CycleConfig cycle, ConfigValidationResult result, string path)
    {
        if (cycle.Schedule is null)
        {
            Error(result, $"{path}.schedule", "Automatic cycles require a schedule.");
            return;
        }

        if (cycle.Schedule.Times.Count == 0)
        {
            Error(result, $"{path}.schedule.times", "Automatic cycles require at least one scheduled time.");
        }

        for (var index = 0; index < cycle.Schedule.Times.Count; index++)
        {
            if (!TimeOnly.TryParse(cycle.Schedule.Times[index], out _))
            {
                Error(result, $"{path}.schedule.times[{index}]", "Invalid time format. Use HH:mm.");
            }
        }
    }

    private static void ValidateStep(
        IrrigationConfig config,
        CycleConfig cycle,
        ConfigValidationResult result,
        string cyclePath,
        int index)
    {
        var step = cycle.Steps[index];
        var path = $"{cyclePath}.steps[{index}]";

        if (step.Zones.Count == 0)
        {
            Error(result, $"{path}.zones", "Step must reference at least one zone.");
        }

        foreach (var zoneId in step.Zones)
        {
            if (!config.Zones.ContainsKey(zoneId))
            {
                Error(result, $"{path}.zones", $"Unknown zone '{zoneId}'.");
            }
        }

        if (cycle.Mode == CycleMode.Manual && step.DurationMinutes is null)
        {
            Error(result, $"{path}.duration_minutes", "Manual steps require duration_minutes.");
        }

        if (step.DurationMinutes is <= 0)
        {
            Error(result, $"{path}.duration_minutes", "Duration must be greater than zero.");
        }
    }

    private static void Error(ConfigValidationResult result, string path, string message) =>
        result.Errors.Add(new ConfigValidationIssue(path, message));

    private static void Warning(ConfigValidationResult result, string path, string message) =>
        result.Warnings.Add(new ConfigValidationIssue(path, message));
}
