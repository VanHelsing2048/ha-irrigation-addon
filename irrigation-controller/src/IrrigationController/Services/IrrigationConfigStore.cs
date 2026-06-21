using System.Text.Json;
using System.Text.Json.Serialization;
using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class IrrigationConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;
    private IrrigationConfig? _cached;

    public IrrigationConfigStore(IConfiguration configuration)
    {
        _path = configuration["IRRIGATION_CONFIG_PATH"]
            ?? Environment.GetEnvironmentVariable("IRRIGATION_CONFIG_PATH")
            ?? "/data/irrigation.json";
    }

    public async Task<IrrigationConfig> GetAsync(CancellationToken cancellationToken)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        return await ReloadAsync(cancellationToken);
    }

    public async Task<IrrigationConfig> ReloadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var sample = CreateSample();
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(sample, JsonOptions), cancellationToken);
            _cached = sample;
            return sample;
        }

        await using var stream = File.OpenRead(_path);
        _cached = await JsonSerializer.DeserializeAsync<IrrigationConfig>(stream, JsonOptions, cancellationToken)
            ?? new IrrigationConfig();
        return _cached;
    }

    private static IrrigationConfig CreateSample() => new()
    {
        Weather = new WeatherConfig
        {
            Entity = "weather.home",
            RainLookaheadHours = 24,
            RainEfficiency = 0.75
        },
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
            },
            ["orto"] = new ZoneConfig
            {
                Name = "Orto",
                Entity = "switch.valvola_orto",
                PrecipitationRateMmH = 8,
                CropCoefficient = 1.05,
                MinMinutes = 5,
                MaxMinutes = 35
            }
        },
        Cycles =
        {
            ["manuale_giardino"] = new CycleConfig
            {
                Name = "Manuale giardino",
                Mode = CycleMode.Manual,
                Steps =
                [
                    new CycleStepConfig { Zones = ["prato"], DurationMinutes = 15 },
                    new CycleStepConfig { Zones = ["orto"], DurationMinutes = 10 }
                ]
            },
            ["automatico_mattina"] = new CycleConfig
            {
                Name = "Automatico mattina",
                Mode = CycleMode.Automatic,
                Schedule = new ScheduleConfig
                {
                    Days = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
                    Times = ["06:30"]
                },
                Steps =
                [
                    new CycleStepConfig { Zones = ["prato"] },
                    new CycleStepConfig { Zones = ["orto"] }
                ]
            }
        }
    };
}
