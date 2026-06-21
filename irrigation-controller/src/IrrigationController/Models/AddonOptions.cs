namespace IrrigationController.Models;

public sealed class AddonOptions
{
    public string IrrigationConfigPath { get; set; } = "/data/irrigation.json";
    public WeatherConfig? Weather { get; set; }
    public MqttDiscoveryConfig? MqttDiscovery { get; set; }
    public SafetyAddonOptions? Safety { get; set; }
    public HydraulicConfig? Hydraulic { get; set; }
}

public sealed class SafetyAddonOptions
{
    public bool TurnOffAllZonesOnStartup { get; set; } = true;
    public bool StopAllKnownZonesOnError { get; set; } = true;
    public bool VerifyZoneStateAfterSwitch { get; set; } = true;
    public int SwitchRetryCount { get; set; } = 2;
    public int SwitchRetryDelayMs { get; set; } = 750;
    public bool ManualRunsIgnoreWeather { get; set; } = true;
    public int MaxZoneMinutes { get; set; } = 60;
}
