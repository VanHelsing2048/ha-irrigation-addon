namespace IrrigationController.Models;

public sealed class IrrigationRuntimeState
{
    public Dictionary<string, double> WaterBalance { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> LastScheduledRuns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, CalibrationResult> Calibrations { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? LastWaterBalanceUpdateDate { get; set; }
    public DiagnosticsState Diagnostics { get; set; } = new();
    public List<IrrigationEvent> Events { get; set; } = [];
}

public sealed class DiagnosticsState
{
    public WeatherDiagnostic? LastWeather { get; set; }
    public DecisionDiagnostic? LastDecision { get; set; }
    public ErrorDiagnostic? LastError { get; set; }
}

public sealed class WeatherDiagnostic
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public double Et0Mm { get; set; }
    public double ExpectedRainMm { get; set; }
    public double EffectiveRainMm { get; set; }
    public int MaxRainProbability { get; set; }
    public bool ShouldSkip { get; set; }
}

public sealed class DecisionDiagnostic
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public string? CycleId { get; set; }
    public string? ZoneId { get; set; }
}

public sealed class ErrorDiagnostic
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Source { get; set; } = "";
    public string Message { get; set; } = "";
}

public sealed class IrrigationEvent
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public string? ZoneId { get; set; }
    public double? AmountMm { get; set; }
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

public sealed class CalibrationCompleteRequest
{
    public int Minutes { get; set; }
    public List<double> MeasurementsMm { get; set; } = [];
}

public sealed class CalibrationResult
{
    public string ZoneId { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public int Minutes { get; set; }
    public List<double> MeasurementsMm { get; set; } = [];
    public double AverageMm { get; set; }
    public double MinMm { get; set; }
    public double MaxMm { get; set; }
    public double PrecipitationRateMmH { get; set; }
    public double DistributionUniformityPercent { get; set; }
    public string Recommendation { get; set; } = "";
}

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
