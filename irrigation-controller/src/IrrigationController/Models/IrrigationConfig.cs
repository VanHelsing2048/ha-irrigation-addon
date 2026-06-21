namespace IrrigationController.Models;

public sealed class IrrigationConfig
{
    public WeatherConfig Weather { get; set; } = new();
    public MqttDiscoveryConfig MqttDiscovery { get; set; } = new();
    public SafetyConfig Safety { get; set; } = new();
    public Dictionary<string, ZoneConfig> Zones { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, CycleConfig> Cycles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class MqttDiscoveryConfig
{
    public bool Enabled { get; set; } = true;
    public string DiscoveryPrefix { get; set; } = "homeassistant";
    public string BaseTopic { get; set; } = "irrigation_controller";
    public int PublishIntervalSeconds { get; set; } = 30;
}

public sealed class WeatherConfig
{
    public string Entity { get; set; } = "weather.home";
    public string ForecastType { get; set; } = "hourly";
    public int RainLookaheadHours { get; set; } = 24;
    public double RainEfficiency { get; set; } = 0.75;
    public double SkipIfExpectedRainMmAbove { get; set; } = 4;
    public int SkipIfRainProbabilityAbove { get; set; } = 70;
    public string? ExternalEt0SensorEntity { get; set; }
}

public sealed class SafetyConfig
{
    public bool OneCycleAtATime { get; set; } = true;
    public bool StopAllKnownZonesOnError { get; set; } = true;
    public bool ManualRunsIgnoreWeather { get; set; } = true;
    public int MaxZoneMinutes { get; set; } = 60;
}

public sealed class ZoneConfig
{
    public string Name { get; set; } = "";
    public string Entity { get; set; } = "";
    public double PrecipitationRateMmH { get; set; } = 10;
    public double CropCoefficient { get; set; } = 1;
    public int MinMinutes { get; set; } = 3;
    public int MaxMinutes { get; set; } = 30;
    public double TargetDeficitMm { get; set; } = 0;
    public string? SoilMoistureEntity { get; set; }
    public double? SkipIfSoilMoistureAbove { get; set; }
}

public sealed class CycleConfig
{
    public string Name { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public CycleMode Mode { get; set; } = CycleMode.Manual;
    public ScheduleConfig? Schedule { get; set; }
    public List<CycleStepConfig> Steps { get; set; } = [];
}

public sealed class ScheduleConfig
{
    public List<DayOfWeek> Days { get; set; } = [];
    public List<string> Times { get; set; } = [];
}

public sealed class CycleStepConfig
{
    public List<string> Zones { get; set; } = [];
    public int? DurationMinutes { get; set; }
}

public enum CycleMode
{
    Manual,
    Automatic
}

public enum TriggerSource
{
    Manual,
    Schedule
}
