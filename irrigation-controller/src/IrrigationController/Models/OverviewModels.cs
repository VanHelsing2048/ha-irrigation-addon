namespace IrrigationController.Models;

public sealed class IrrigationOverview
{
    public RunnerSnapshot Runner { get; set; } = new();
    public string ExpectedEndText { get; set; } = "-";
    public ConfigValidationResult Validation { get; set; } = new();
    public string LastWaterBalanceUpdateDate { get; set; } = "-";
    public DiagnosticsState Diagnostics { get; set; } = new();
    public List<IrrigationEvent> RecentEvents { get; set; } = [];
    public List<CycleOverview> Cycles { get; set; } = [];
    public List<ZoneOverview> Zones { get; set; } = [];
}

public sealed class CycleOverview
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public CycleMode Mode { get; set; }
    public bool Enabled { get; set; }
    public DateTimeOffset? NextRun { get; set; }
    public string NextRunText { get; set; } = "-";
}

public sealed class ZoneOverview
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Entity { get; set; } = "";
    public string State { get; set; } = "unknown";
    public string StateClass { get; set; } = "unknown";
    public double WaterBalanceMm { get; set; }
    public double? CalibratedPrecipitationRateMmH { get; set; }
    public string CalibrationText { get; set; } = "-";
}
