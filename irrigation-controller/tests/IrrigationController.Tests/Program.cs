using IrrigationController.Models;
using IrrigationController.Services;
using System.Text.Json;

var tests = new (string Name, Action Test)[]
{
    ("calibration computes precipitation rate", CalibrationComputesPrecipitationRate),
    ("calibration rejects invalid input", CalibrationRejectsInvalidInput),
    ("config validator catches unknown cycle zone", ConfigValidatorCatchesUnknownCycleZone),
    ("config validator accepts basic sample", ConfigValidatorAcceptsBasicSample),
    ("config validator accepts valve entity", ConfigValidatorAcceptsValveEntity),
    ("config validator accepts empty zone setup", ConfigValidatorAcceptsEmptyZoneSetup),
    ("config validator catches invalid hydraulic policy", ConfigValidatorCatchesInvalidHydraulicPolicy),
    ("config validator accepts interval schedule", ConfigValidatorAcceptsIntervalSchedule),
    ("config validator catches invalid interval schedule", ConfigValidatorCatchesInvalidIntervalSchedule),
    ("config validator accepts master valve", ConfigValidatorAcceptsMasterValve),
    ("schedule calculator handles interval dates", ScheduleCalculatorHandlesIntervalDates),
    ("schedule calculator finds next run", ScheduleCalculatorFindsNextRun),
    ("duration calculator projects daily ET for dry-run", DurationCalculatorProjectsDailyEtForDryRun),
    ("duration calculator avoids double daily ET", DurationCalculatorAvoidsDoubleDailyEt),
    ("forecast parser reads service response", ForecastParserReadsServiceResponse),
    ("decision plan requests daily forecasts", DecisionPlanRequestsDailyForecasts),
    ("ui uses escaped action handlers", UiUsesEscapedActionHandlers),
    ("ui sends save audit headers", UiSendsSaveAuditHeaders),
    ("ui contains cycle event register", AssertUiContainsCycleRegister),
    ("ui contains weather entity picker", AssertUiContainsWeatherEntityPicker),
    ("ui contains dashboard weather summary", AssertUiContainsDashboardWeatherSummary),
    ("ui contains cycle step editor", AssertUiContainsCycleStepEditor),
    ("ui contains master valve setting", AssertUiContainsMasterValveSetting),
    ("ui contains dry run action", AssertUiContainsDryRunAction),
    ("ui contains polished shell", AssertUiContainsPolishedShell),
    ("ui contains operation summaries", AssertUiContainsOperationSummaries),
    ("ui contains inline validation", AssertUiContainsInlineValidation),
    ("ui contains cycle decision preview", AssertUiContainsCycleDecisionPreview),
    ("ui contains weather and plant overview", AssertUiContainsWeatherAndPlantOverview)
};

var failures = 0;
foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failures > 0)
{
    Environment.ExitCode = 1;
}

static void CalibrationComputesPrecipitationRate()
{
    var result = CalibrationCalculator.Calculate("prato", new CalibrationCompleteRequest
    {
        Minutes = 10,
        MeasurementsMm = [1.8, 2.1, 1.6, 2.0]
    }) ?? throw new InvalidOperationException("Expected calibration result.");

    AssertEqual(1.88, result.AverageMm, 0.01);
    AssertEqual(11.25, result.PrecipitationRateMmH, 0.01);
    AssertEqual(85.3, result.DistributionUniformityPercent, 0.1);
}

static void CalibrationRejectsInvalidInput()
{
    var result = CalibrationCalculator.Calculate("prato", new CalibrationCompleteRequest
    {
        Minutes = 10,
        MeasurementsMm = [0, -1]
    });

    if (result is not null)
    {
        throw new InvalidOperationException("Expected invalid calibration to return null.");
    }
}

static void ConfigValidatorCatchesUnknownCycleZone()
{
    var config = BasicConfig();
    config.Cycles["bad"] = new CycleConfig
    {
        Name = "Bad",
        Mode = CycleMode.Manual,
        Steps = [new CycleStepConfig { Zones = ["missing"], DurationMinutes = 5 }]
    };

    var result = new IrrigationConfigValidator().Validate(config);
    if (result.IsValid)
    {
        throw new InvalidOperationException("Expected validation error.");
    }
}

static void ConfigValidatorAcceptsBasicSample()
{
    var result = new IrrigationConfigValidator().Validate(BasicConfig());
    if (!result.IsValid)
    {
        throw new InvalidOperationException(result.Errors[0].Message);
    }
}

static void ConfigValidatorAcceptsValveEntity()
{
    var config = BasicConfig();
    config.Zones["prato"].Entity = "valve.prato";

    var result = new IrrigationConfigValidator().Validate(config);
    if (!result.IsValid)
    {
        throw new InvalidOperationException(result.Errors[0].Message);
    }

    if (result.Warnings.Any(warning => warning.Path == "zones.prato.entity"))
    {
        throw new InvalidOperationException("Expected valve entity to be accepted without entity warning.");
    }
}

static void ConfigValidatorAcceptsEmptyZoneSetup()
{
    var config = BasicConfig();
    config.Zones.Clear();
    config.Cycles.Clear();

    var result = new IrrigationConfigValidator().Validate(config);
    if (!result.IsValid)
    {
        throw new InvalidOperationException(result.Errors[0].Message);
    }

    if (!result.Warnings.Any(warning => warning.Path == "zones"))
    {
        throw new InvalidOperationException("Expected empty zones to produce a warning.");
    }
}

static void ConfigValidatorCatchesInvalidHydraulicPolicy()
{
    var config = BasicConfig();
    config.Hydraulic.MaxParallelZones = 0;

    var result = new IrrigationConfigValidator().Validate(config);
    if (result.IsValid)
    {
        throw new InvalidOperationException("Expected hydraulic validation error.");
    }
}

static void UiUsesEscapedActionHandlers()
{
    var html = new UiRenderer().Render();

    if (!html.Contains("function action(name, value)", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected action helper to be rendered.");
    }

    if (!html.Contains("onclick=\"${esc(action('saveZone', id))}\"", StringComparison.Ordinal)
        || !html.Contains("onclick=\"${esc(action('saveCycle', id))}\"", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected save buttons to use escaped action handlers.");
    }

    if (html.Contains("onclick=\"saveZone(${js(id)})", StringComparison.Ordinal)
        || html.Contains("onclick=\"saveCycle(${js(id)})", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Found an unescaped draft save handler.");
    }
}

static void UiSendsSaveAuditHeaders()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "X-Irrigation-Action",
        "X-Irrigation-Message",
        "X-Irrigation-Cycle",
        "Zona salvata",
        "Ciclo salvato",
        "Meteo salvato",
        "Impianto salvato",
        "JSON salvato"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected UI to contain '{value}'.");
        }
    }
}

static void ConfigValidatorAcceptsIntervalSchedule()
{
    var config = BasicConfig();
    config.Cycles["manuale"].Mode = CycleMode.Automatic;
    config.Cycles["manuale"].Schedule = new ScheduleConfig
    {
        StartDate = "2026-06-24",
        EveryDays = 2,
        Times = ["06:00"]
    };

    var result = new IrrigationConfigValidator().Validate(config);
    if (!result.IsValid)
    {
        throw new InvalidOperationException(result.Errors[0].Message);
    }
}

static void ConfigValidatorCatchesInvalidIntervalSchedule()
{
    var config = BasicConfig();
    config.Cycles["manuale"].Mode = CycleMode.Automatic;
    config.Cycles["manuale"].Schedule = new ScheduleConfig
    {
        StartDate = "not-a-date",
        EveryDays = 0,
        Times = ["06:00"]
    };

    var result = new IrrigationConfigValidator().Validate(config);
    if (result.IsValid)
    {
        throw new InvalidOperationException("Expected invalid interval schedule to fail.");
    }
}

static void ConfigValidatorAcceptsMasterValve()
{
    var config = BasicConfig();
    config.Hydraulic.MasterValveEntity = "valve.master_irrigazione";

    var result = new IrrigationConfigValidator().Validate(config);
    if (!result.IsValid)
    {
        throw new InvalidOperationException(result.Errors[0].Message);
    }

    if (result.Warnings.Any(warning => warning.Path == "hydraulic.master_valve_entity"))
    {
        throw new InvalidOperationException("Expected master valve entity to be accepted without warning.");
    }
}

static void ScheduleCalculatorHandlesIntervalDates()
{
    var schedule = new ScheduleConfig
    {
        StartDate = "2026-06-24",
        EveryDays = 3,
        Times = ["06:00"]
    };

    if (!ScheduleCalculator.IsScheduledDate(schedule, new DateOnly(2026, 6, 24)))
    {
        throw new InvalidOperationException("Expected start date to be scheduled.");
    }

    if (!ScheduleCalculator.IsScheduledDate(schedule, new DateOnly(2026, 6, 27)))
    {
        throw new InvalidOperationException("Expected third day after start date to be scheduled.");
    }

    if (ScheduleCalculator.IsScheduledDate(schedule, new DateOnly(2026, 6, 25)))
    {
        throw new InvalidOperationException("Expected date outside interval to be skipped.");
    }

    if (ScheduleCalculator.IsScheduledDate(schedule, new DateOnly(2026, 6, 23)))
    {
        throw new InvalidOperationException("Expected date before start date to be skipped.");
    }
}

static void ForecastParserReadsServiceResponse()
{
    using var wrapped = JsonDocument.Parse("""
    {
      "changed_states": [],
      "service_response": {
        "weather.forecast_home": {
          "forecast": [
            { "datetime": "2026-06-26T10:00:00+00:00", "condition": "sunny", "precipitation": 0.0 }
          ]
        }
      }
    }
    """);

    var wrappedItems = HomeAssistantForecastReader
        .EnumerateForecastItems(wrapped.RootElement, "weather.forecast_home")
        .ToList();

    if (wrappedItems.Count != 1 || wrappedItems[0].GetProperty("condition").GetString() != "sunny")
    {
        throw new InvalidOperationException("Expected service_response forecast to be read.");
    }

    using var direct = JsonDocument.Parse("""
    {
      "weather.forecast_home": {
        "forecast": [
          { "datetime": "2026-06-26T11:00:00+00:00", "condition": "partlycloudy", "precipitation": 0.0 }
        ]
      }
    }
    """);

    var directItems = HomeAssistantForecastReader
        .EnumerateForecastItems(direct.RootElement, "weather.forecast_home")
        .ToList();

    if (directItems.Count != 1 || directItems[0].GetProperty("condition").GetString() != "partlycloudy")
    {
        throw new InvalidOperationException("Expected direct forecast to remain supported.");
    }
}

static void ScheduleCalculatorFindsNextRun()
{
    var cycle = new CycleConfig
    {
        Enabled = true,
        Mode = CycleMode.Automatic,
        Schedule = new ScheduleConfig
        {
            StartDate = "2026-06-24",
            EveryDays = 2,
            Times = ["06:00", "21:30"]
        }
    };

    var next = ScheduleCalculator.CalculateNextRun(cycle, new DateTimeOffset(2026, 6, 24, 7, 0, 0, TimeSpan.FromHours(2)));

    if (next is null || next.Value.Date != new DateTime(2026, 6, 24) || next.Value.Hour != 21 || next.Value.Minute != 30)
    {
        throw new InvalidOperationException($"Expected next run on 2026-06-24 21:30, got {next}.");
    }

    next = ScheduleCalculator.CalculateNextRun(cycle, new DateTimeOffset(2026, 6, 24, 22, 0, 0, TimeSpan.FromHours(2)));
    if (next is null || next.Value.Date != new DateTime(2026, 6, 26) || next.Value.Hour != 6)
    {
        throw new InvalidOperationException($"Expected next interval run on 2026-06-26 06:00, got {next}.");
    }
}

static void DurationCalculatorProjectsDailyEtForDryRun()
{
    var config = BasicConfig();
    var zone = config.Zones["prato"];
    var state = new IrrigationRuntimeState();
    var duration = IrrigationDurationCalculator.EstimateAutomaticDuration(
        config,
        state,
        "prato",
        zone,
        new WeatherAdjustment(Et0Mm: 4, ExpectedRainMm: 0, EffectiveRainMm: 0, MaxRainProbability: 0, ShouldSkip: false),
        new DateOnly(2026, 6, 24));

    if (duration <= TimeSpan.Zero)
    {
        throw new InvalidOperationException("Expected dry-run automatic duration to include today's ET projection.");
    }
}

static void DurationCalculatorAvoidsDoubleDailyEt()
{
    var config = BasicConfig();
    var zone = config.Zones["prato"];
    var state = new IrrigationRuntimeState
    {
        LastWaterBalanceUpdateDate = "2026-06-24",
        WaterBalance = { ["prato"] = 0 }
    };

    var duration = IrrigationDurationCalculator.EstimateAutomaticDuration(
        config,
        state,
        "prato",
        zone,
        new WeatherAdjustment(Et0Mm: 4, ExpectedRainMm: 0, EffectiveRainMm: 0, MaxRainProbability: 0, ShouldSkip: false),
        new DateOnly(2026, 6, 24));

    if (duration != TimeSpan.Zero)
    {
        throw new InvalidOperationException("Expected already-updated daily balance to avoid adding ET twice.");
    }
}

static void DecisionPlanRequestsDailyForecasts()
{
    var source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "../../../../../src/IrrigationController/Services/DecisionPlanService.cs"));
    var expected = new[]
    {
        "ReadForecastsAsync(config.Weather.Entity, config.Weather.ForecastType",
        "ReadForecastsAsync(config.Weather.Entity, \"daily\"",
        "ForecastCount = dayForecasts.Count",
        "HasForecast = dayForecasts.Count > 0"
    };

    foreach (var value in expected)
    {
        if (!source.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected decision plan forecast marker '{value}'.");
        }
    }
}

static void AssertUiContainsCycleRegister()
{
    var html = new UiRenderer().Render();
    if (!html.Contains("Registro ciclo", StringComparison.Ordinal)
        || !html.Contains("cycleEventRegister", StringComparison.Ordinal)
        || !html.Contains("cycle_id", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected UI to render per-cycle event register.");
    }
}

static void AssertUiContainsWeatherEntityPicker()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "api('/api/entities/weather')",
        "let weatherEntities = []",
        "weather-entities",
        "weatherEntityField('weather-entity'",
        "/api/weather/forecast-check"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected weather entity picker marker '{value}'.");
        }
    }
}

static void AssertUiContainsDashboardWeatherSummary()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "Meteo e decisione",
        "current-weather-card",
        "Fonte forecast",
        "renderPlanPanel",
        "dayPlan",
        "formatWeatherState",
        "Nessuna previsione ricevuta da Home Assistant",
        "previsioni ricevute",
        "zoneDecisionCard",
        "current_deficit_mm",
        "irrigation_deficit_mm",
        "iconSvg",
        "<svg viewBox=\"0 0 24 24\"><circle cx=\"12\" cy=\"12\" r=\"4\"></circle>"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected dashboard weather summary marker '{value}'.");
        }
    }
}

static void AssertUiContainsCycleStepEditor()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "Aggiungi step",
        "Tempo hh:mm:ss",
        "collectCycleSteps",
        "parseDurationInput",
        "Orari partenza",
        "type=\"time\"",
        "collectCycleTimes",
        "addCycleTime",
        "Data inizio alternanza",
        "Ogni quanti giorni"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected cycle step editor marker '{value}'.");
        }
    }
}

static void AssertUiContainsMasterValveSetting()
{
    var html = new UiRenderer().Render();
    if (!html.Contains("Valvola master", StringComparison.Ordinal)
        || !html.Contains("hyd-master", StringComparison.Ordinal)
        || !html.Contains("master_valve_entity", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected master valve setting in UI.");
    }
}

static void AssertUiContainsDryRunAction()
{
    var html = new UiRenderer().Render();
    if (!html.Contains("Simula", StringComparison.Ordinal)
        || !html.Contains("dryRunCycle", StringComparison.Ordinal)
        || !html.Contains("/dry-run", StringComparison.Ordinal)
        || !html.Contains("runSimulation", StringComparison.Ordinal)
        || !html.Contains("/api/simulation/", StringComparison.Ordinal)
        || !html.Contains("Timeline", StringComparison.Ordinal)
        || !html.Contains("simulation-zone-grid", StringComparison.Ordinal)
        || !html.Contains("formula-box", StringComparison.Ordinal)
        || !html.Contains("formula_text", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected dry-run action in UI.");
    }
}

static void AssertUiContainsPolishedShell()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "nav-code",
        "Ingress UI",
        "dashboardHero",
        "renderSimulation",
        "Simulazione dry-run",
        "renderSetup",
        "Configurazione guidata",
        "setupCheck",
        "zonePresets",
        "createSetupZone",
        "createSetupCycle",
        "saveSetupWeather",
        "Calibrazione guidata",
        "completeCalibrationGuide",
        "startCalibrationGuide",
        "da calibrare",
        "Calibrazione",
        "status-panel",
        "quick-metrics",
        "icon-metric",
        "icon-sun",
        "event-register",
        "@media (max-width: 700px)",
        "position: fixed; left: 0; right: 0; bottom: 0",
        "<details class=\"event-register\"",
        "advancedToggle",
        "irrigation.advancedMode",
        "toggleAdvancedMode",
        "visiblePages",
        "Modalita base",
        "emptyState",
        "Nessuna zona configurata",
        "Nessun ciclo configurato",
        "box-shadow: var(--shadow)"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected polished UI marker '{value}'.");
        }
    }
}

static void AssertUiContainsOperationSummaries()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "Zone salvate",
        "Calibrate",
        "Cicli abilitati",
        "Automatici",
        "eventLabel",
        "Master on"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected operation summary marker '{value}'.");
        }
    }
}

static void AssertUiContainsInlineValidation()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "let lastValidation = null",
        "validationPanel('zones')",
        "validationPanel('cycles')",
        "Salvataggio non riuscito",
        "Correggi questi punti e salva di nuovo."
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected inline validation marker '{value}'.");
        }
    }

    if (html.Contains("alert(details)", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected validation errors to be rendered inline instead of shown with alert().");
    }
}

static void AssertUiContainsCycleDecisionPreview()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "Anteprima decisionale",
        "cycleDecisionPreview",
        "cycleDecisionForDay",
        "Nessuna partenza prevista",
        "Nessuna valutazione pianificata"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected cycle decision preview marker '{value}'.");
        }
    }
}

static void AssertUiContainsWeatherAndPlantOverview()
{
    var html = new UiRenderer().Render();
    var expected = new[]
    {
        "Meteo operativo",
        "weatherSettingsOverview",
        "Dati ricevuti da Home Assistant",
        "weatherDiagnosticsPanel",
        "Verifica forecast Home Assistant",
        "checkWeatherForecast",
        "weatherForecastCheckPanel",
        "tomorrow_available",
        "tomorrow_rain_mm",
        "forecast_records",
        "first_forecast_at",
        "Ultima ET0",
        "Pioggia utile",
        "Schema impianto",
        "plantFlow",
        "Zone collegate",
        "Master attiva"
    };

    foreach (var value in expected)
    {
        if (!html.Contains(value, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected weather/plant overview marker '{value}'.");
        }
    }

    if (html.Contains("['N/D'", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Expected weather icon fallback to avoid N/D labels.");
    }
}

static IrrigationConfig BasicConfig() => new()
{
    Weather = new WeatherConfig { Entity = "weather.home" },
    Zones =
    {
        ["prato"] = new ZoneConfig
        {
            Name = "Prato",
            Entity = "switch.valvola_prato",
            PrecipitationRateMmH = 12,
            CropCoefficient = 0.8,
            MinMinutes = 4,
            MaxMinutes = 25
        }
    },
    Cycles =
    {
        ["manuale"] = new CycleConfig
        {
            Name = "Manuale",
            Mode = CycleMode.Manual,
            Steps = [new CycleStepConfig { Zones = ["prato"], DurationMinutes = 10 }]
        }
    }
};

static void AssertEqual(double expected, double actual, double tolerance)
{
    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }
}
