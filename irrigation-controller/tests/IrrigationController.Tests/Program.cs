using IrrigationController.Models;
using IrrigationController.Services;

var tests = new (string Name, Action Test)[]
{
    ("calibration computes precipitation rate", CalibrationComputesPrecipitationRate),
    ("calibration rejects invalid input", CalibrationRejectsInvalidInput),
    ("config validator catches unknown cycle zone", ConfigValidatorCatchesUnknownCycleZone),
    ("config validator accepts basic sample", ConfigValidatorAcceptsBasicSample),
    ("config validator accepts valve entity", ConfigValidatorAcceptsValveEntity),
    ("config validator catches invalid hydraulic policy", ConfigValidatorCatchesInvalidHydraulicPolicy)
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
