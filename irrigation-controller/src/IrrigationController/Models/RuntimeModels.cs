namespace IrrigationController.Models;

public sealed class IrrigationRuntimeState
{
    public Dictionary<string, double> WaterBalance { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> LastScheduledRuns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class RunnerSnapshot
{
    public bool IsRunning { get; set; }
    public string? CycleId { get; set; }
    public string? CycleName { get; set; }
    public string? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? ExpectedEndAt { get; set; }
    public string? Status { get; set; }
}

public sealed record CommandResult(bool Success, string Message);

public sealed record WeatherAdjustment(
    double Et0Mm,
    double ExpectedRainMm,
    double EffectiveRainMm,
    int MaxRainProbability,
    bool ShouldSkip);

public sealed class ControllerMqttState
{
    public string Status { get; set; } = "idle";
    public bool Running { get; set; }
    public string ActiveCycle { get; set; } = "";
    public string ActiveZone { get; set; } = "";
    public int RemainingSeconds { get; set; }
    public string NextCycle { get; set; } = "";
    public string NextRun { get; set; } = "";
    public int ConfigErrors { get; set; }
    public int ConfigWarnings { get; set; }
}

public sealed class ConfigValidationResult
{
    public List<ConfigValidationIssue> Errors { get; set; } = [];
    public List<ConfigValidationIssue> Warnings { get; set; } = [];
    public bool IsValid => Errors.Count == 0;
}

public sealed record ConfigValidationIssue(string Path, string Message);
