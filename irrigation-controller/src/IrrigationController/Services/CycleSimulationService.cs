using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class CycleSimulationService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly IrrigationConfigValidator _validator;
    private readonly WeatherAdjustmentService _weather;
    private readonly HomeAssistantClient _homeAssistant;

    public CycleSimulationService(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        IrrigationConfigValidator validator,
        WeatherAdjustmentService weather,
        HomeAssistantClient homeAssistant)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _validator = validator;
        _weather = weather;
        _homeAssistant = homeAssistant;
    }

    public async Task<CycleSimulationResult> SimulateAsync(string cycleId, CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);
        if (!validation.IsValid)
        {
            return CycleSimulationResult.Fail(cycleId, $"Configurazione non valida: {validation.Errors[0].Path} - {validation.Errors[0].Message}");
        }

        if (!config.Cycles.TryGetValue(cycleId, out var cycle))
        {
            return CycleSimulationResult.Fail(cycleId, $"Ciclo sconosciuto: {cycleId}");
        }

        var usesWeather = cycle.Mode == CycleMode.Automatic || !config.Safety.ManualRunsIgnoreWeather;
        var adjustment = usesWeather
            ? await _weather.CalculateAsync(config, cancellationToken)
            : new WeatherAdjustment(0, 0, 0, 0, false);

        var result = new CycleSimulationResult
        {
            Success = true,
            CycleId = cycleId,
            CycleName = cycle.Name,
            Mode = cycle.Mode.ToString(),
            UsesWeather = usesWeather,
            HasMasterValve = !string.IsNullOrWhiteSpace(config.Hydraulic.MasterValveEntity),
            MasterValveEntity = config.Hydraulic.MasterValveEntity ?? "",
            Weather = new SimulationWeatherSummary
            {
                ForecastAvailable = adjustment.ForecastAvailable,
                ForecastType = adjustment.ForecastType,
                ForecastRecords = adjustment.ForecastRecords,
                Et0Mm = Math.Round(adjustment.Et0Mm, 1),
                ExpectedRainMm = Math.Round(adjustment.ExpectedRainMm, 1),
                EffectiveRainMm = Math.Round(adjustment.EffectiveRainMm, 1),
                MaxRainProbability = adjustment.MaxRainProbability,
                ShouldSkip = adjustment.ShouldSkip,
                Message = adjustment.Message
            }
        };

        result.Timeline.Add(new SimulationTimelineItem("start", "Inizio simulazione", "Nessuna valvola verra comandata.", "INFO"));

        if (adjustment.ShouldSkip)
        {
            result.Timeline.Add(new SimulationTimelineItem(
                "skip",
                "Ciclo saltato",
                $"Meteo oltre soglia: {adjustment.ExpectedRainMm:0.0} mm, {adjustment.MaxRainProbability}%.",
                "SKIP"));
            result.Summary = SimulationSummary.From(result);
            return result;
        }

        var state = await _stateStore.GetAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        var stepIndex = 0;

        foreach (var step in cycle.Steps)
        {
            stepIndex++;
            foreach (var zoneId in step.Zones)
            {
                if (!config.Zones.TryGetValue(zoneId, out var zone))
                {
                    result.Timeline.Add(new SimulationTimelineItem("warning", $"Zona mancante: {zoneId}", "La zona non e presente in configurazione.", "SKIP"));
                    continue;
                }

                var duration = await ResolveDurationAsync(config, state, cycle, step, zoneId, zone, adjustment, today, cancellationToken);
                var seconds = Math.Max(0, (int)Math.Round(duration.TotalSeconds));
                state.WaterBalance.TryGetValue(zoneId, out var currentDeficit);
                var cropEt = cycle.Mode == CycleMode.Automatic && state.LastWaterBalanceUpdateDate != today.ToString("yyyy-MM-dd")
                    ? adjustment.Et0Mm * zone.CropCoefficient
                    : 0;
                var effectiveRain = cycle.Mode == CycleMode.Automatic && state.LastWaterBalanceUpdateDate != today.ToString("yyyy-MM-dd")
                    ? adjustment.EffectiveRainMm
                    : 0;
                var projectedDeficit = cycle.Mode == CycleMode.Automatic
                    ? Math.Max(0, currentDeficit + cropEt - effectiveRain)
                    : currentDeficit;
                var irrigationDeficit = cycle.Mode == CycleMode.Automatic
                    ? Math.Max(0, projectedDeficit - zone.TargetDeficitMm)
                    : 0;
                var waterMm = seconds <= 0 ? 0 : duration.TotalHours * Math.Max(0.1, zone.PrecipitationRateMmH);
                var status = seconds <= 0 ? "skipped" : "planned";
                var icon = seconds <= 0 ? "OK" : "DROP";
                var reason = seconds <= 0
                    ? "Durata calcolata zero: la zona risulta gia coperta."
                    : $"Irrigazione prevista per {FormatDuration(duration)}.";
                var formula = BuildFormulaText(config, state, cycle, step, zone, today, currentDeficit, cropEt, effectiveRain, projectedDeficit, irrigationDeficit, duration);

                result.Zones.Add(new SimulationZone
                {
                    ZoneId = zoneId,
                    Name = zone.Name,
                    Entity = zone.Entity,
                    Step = stepIndex,
                    Status = status,
                    Icon = icon,
                    DurationSeconds = seconds,
                    DurationText = seconds <= 0 ? "0:00" : FormatDuration(duration),
                    WaterMm = Math.Round(waterMm, 1),
                    CurrentDeficitMm = Math.Round(currentDeficit, 1),
                    CropEtMm = Math.Round(cropEt, 1),
                    EffectiveRainMm = Math.Round(effectiveRain, 1),
                    ProjectedDeficitMm = Math.Round(projectedDeficit, 1),
                    IrrigationDeficitMm = Math.Round(irrigationDeficit, 1),
                    Reason = reason,
                    FormulaText = formula
                });

                if (seconds <= 0)
                {
                    result.Timeline.Add(new SimulationTimelineItem("zone_skip", zone.Name, reason, "OK", zoneId, seconds));
                    continue;
                }

                if (result.HasMasterValve)
                {
                    result.Timeline.Add(new SimulationTimelineItem("master_on", "Master ON", config.Hydraulic.MasterValveEntity ?? "", "BAL"));
                }

                result.Timeline.Add(new SimulationTimelineItem("zone", zone.Name, reason, "DROP", zoneId, seconds));

                if (config.Hydraulic.PauseBetweenZonesSeconds > 0)
                {
                    result.Timeline.Add(new SimulationTimelineItem(
                        "pause",
                        "Pausa",
                        $"{config.Hydraulic.PauseBetweenZonesSeconds} sec.",
                        "INFO",
                        zoneId,
                        config.Hydraulic.PauseBetweenZonesSeconds));
                }

                if (result.HasMasterValve)
                {
                    result.Timeline.Add(new SimulationTimelineItem("master_off", "Master OFF", "Chiusura master dopo zona.", "BAL"));
                }
            }
        }

        result.Timeline.Add(new SimulationTimelineItem("end", "Fine simulazione", "Dry-run completato senza comandi reali.", "OK"));
        result.Summary = SimulationSummary.From(result);
        return result;
    }

    private async Task<TimeSpan> ResolveDurationAsync(
        IrrigationConfig config,
        IrrigationRuntimeState state,
        CycleConfig cycle,
        CycleStepConfig step,
        string zoneId,
        ZoneConfig zone,
        WeatherAdjustment adjustment,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        if (cycle.Mode == CycleMode.Manual)
        {
            var manualDuration = step.DurationSeconds is > 0
                ? TimeSpan.FromSeconds(step.DurationSeconds.Value)
                : TimeSpan.FromMinutes(step.DurationMinutes ?? zone.MinMinutes);
            var maxDuration = TimeSpan.FromMinutes(config.Safety.MaxZoneMinutes);
            return manualDuration > maxDuration ? maxDuration : manualDuration;
        }

        if (!string.IsNullOrWhiteSpace(zone.SoilMoistureEntity) && zone.SkipIfSoilMoistureAbove is not null)
        {
            var moisture = await _homeAssistant.GetNumericStateAsync(zone.SoilMoistureEntity, cancellationToken);
            if (moisture >= zone.SkipIfSoilMoistureAbove)
            {
                return TimeSpan.Zero;
            }
        }

        return IrrigationDurationCalculator.EstimateAutomaticDuration(config, state, zoneId, zone, adjustment, today);
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");

    private static string BuildFormulaText(
        IrrigationConfig config,
        IrrigationRuntimeState state,
        CycleConfig cycle,
        CycleStepConfig step,
        ZoneConfig zone,
        DateOnly today,
        double currentDeficit,
        double cropEt,
        double effectiveRain,
        double projectedDeficit,
        double irrigationDeficit,
        TimeSpan duration)
    {
        var maxMinutes = Math.Min(zone.MaxMinutes, config.Safety.MaxZoneMinutes);
        if (cycle.Mode == CycleMode.Manual)
        {
            var configuredSeconds = step.DurationSeconds is > 0
                ? step.DurationSeconds.Value
                : (step.DurationMinutes ?? zone.MinMinutes) * 60;
            var cappedSeconds = Math.Min(configuredSeconds, config.Safety.MaxZoneMinutes * 60);
            var capText = configuredSeconds == cappedSeconds
                ? "nessun limite applicato"
                : $"limitato a {config.Safety.MaxZoneMinutes} min max sicurezza";
            return $"Manuale: tempo step {FormatSeconds(configuredSeconds)} -> {FormatSeconds(cappedSeconds)} ({capText}).";
        }

        var balanceAlreadyUpdated = state.LastWaterBalanceUpdateDate == today.ToString("yyyy-MM-dd");
        var updateText = balanceAlreadyUpdated
            ? "bilancio odierno gia aggiornato: ET e pioggia non risommati"
            : $"ET zona {cropEt:0.0} mm - pioggia utile {effectiveRain:0.0} mm";
        var rawMinutes = irrigationDeficit <= 0
            ? 0
            : irrigationDeficit / Math.Max(0.1, zone.PrecipitationRateMmH) * 60;
        var clampedMinutes = duration.TotalMinutes;
        var clampText = rawMinutes <= 0
            ? "zona coperta"
            : $"clamp tra min {zone.MinMinutes} e max {maxMinutes} min";

        return $"Automatico: ({currentDeficit:0.0} mm deficit + {cropEt:0.0} mm ET - {effectiveRain:0.0} mm pioggia) = {projectedDeficit:0.0} mm; " +
               $"{projectedDeficit:0.0} - target {zone.TargetDeficitMm:0.0} = {irrigationDeficit:0.0} mm da reintegrare; " +
               $"{irrigationDeficit:0.0} / {Math.Max(0.1, zone.PrecipitationRateMmH):0.0} mm/h * 60 = {rawMinutes:0.#} min -> {FormatDuration(duration)} ({clampText}; {updateText}).";
    }

    private static string FormatSeconds(int seconds)
    {
        var duration = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return FormatDuration(duration);
    }
}

public sealed class CycleSimulationResult
{
    public bool Success { get; set; }
    public string CycleId { get; set; } = "";
    public string CycleName { get; set; } = "";
    public string Mode { get; set; } = "";
    public bool UsesWeather { get; set; }
    public bool HasMasterValve { get; set; }
    public string MasterValveEntity { get; set; } = "";
    public string Message { get; set; } = "";
    public SimulationSummary Summary { get; set; } = new();
    public SimulationWeatherSummary Weather { get; set; } = new();
    public List<SimulationTimelineItem> Timeline { get; set; } = [];
    public List<SimulationZone> Zones { get; set; } = [];

    public static CycleSimulationResult Fail(string cycleId, string message) => new()
    {
        Success = false,
        CycleId = cycleId,
        Message = message
    };
}

public sealed class SimulationSummary
{
    public int TotalSeconds { get; set; }
    public string TotalDurationText { get; set; } = "0:00";
    public int PlannedZones { get; set; }
    public int SkippedZones { get; set; }
    public double TotalWaterMm { get; set; }

    public static SimulationSummary From(CycleSimulationResult result)
    {
        var totalSeconds = result.Zones.Sum(zone => zone.DurationSeconds);
        return new SimulationSummary
        {
            TotalSeconds = totalSeconds,
            TotalDurationText = Format(TimeSpan.FromSeconds(totalSeconds)),
            PlannedZones = result.Zones.Count(zone => zone.DurationSeconds > 0),
            SkippedZones = result.Zones.Count(zone => zone.DurationSeconds <= 0),
            TotalWaterMm = Math.Round(result.Zones.Sum(zone => zone.WaterMm), 1)
        };
    }

    private static string Format(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");
}

public sealed class SimulationWeatherSummary
{
    public bool ForecastAvailable { get; set; }
    public string ForecastType { get; set; } = "";
    public int ForecastRecords { get; set; }
    public double Et0Mm { get; set; }
    public double ExpectedRainMm { get; set; }
    public double EffectiveRainMm { get; set; }
    public int MaxRainProbability { get; set; }
    public bool ShouldSkip { get; set; }
    public string Message { get; set; } = "";
}

public sealed record SimulationTimelineItem(
    string Type,
    string Title,
    string Text,
    string Icon,
    string? ZoneId = null,
    int DurationSeconds = 0);

public sealed class SimulationZone
{
    public string ZoneId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Entity { get; set; } = "";
    public int Step { get; set; }
    public string Status { get; set; } = "";
    public string Icon { get; set; } = "";
    public int DurationSeconds { get; set; }
    public string DurationText { get; set; } = "";
    public double WaterMm { get; set; }
    public double CurrentDeficitMm { get; set; }
    public double CropEtMm { get; set; }
    public double EffectiveRainMm { get; set; }
    public double ProjectedDeficitMm { get; set; }
    public double IrrigationDeficitMm { get; set; }
    public string Reason { get; set; } = "";
    public string FormulaText { get; set; } = "";
}
