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
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IrrigationConfig? _cached;

    public IrrigationConfigStore(IConfiguration configuration)
    {
        _path = configuration["IRRIGATION_CONFIG_PATH"]
            ?? Environment.GetEnvironmentVariable("IRRIGATION_CONFIG_PATH")
            ?? "/data/irrigation.json";
    }

    public async Task<IrrigationConfig> GetAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
        if (_cached is not null)
        {
            return _cached;
        }

        _cached = await LoadAsync(cancellationToken);
        return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IrrigationConfig> ReloadAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
        _cached = await LoadAsync(cancellationToken);
        return _cached;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(IrrigationConfig config, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            if (File.Exists(_path))
            {
                var backupPath = $"{_path}.{DateTimeOffset.Now:yyyyMMddHHmmssfff}.bak";
                File.Copy(_path, backupPath, overwrite: false);
            }

            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(config, JsonOptions), cancellationToken);
            _cached = config;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IrrigationConfig> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var sample = CreateSample();
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(sample, JsonOptions), cancellationToken);
            return sample;
        }

        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<IrrigationConfig>(stream, JsonOptions, cancellationToken)
            ?? new IrrigationConfig();
    }

    private static IrrigationConfig CreateSample() => new()
    {
        Weather = new WeatherConfig
        {
            Entity = "weather.home",
            RainLookaheadHours = 24,
            RainEfficiency = 0.75
        },
        MqttDiscovery = new MqttDiscoveryConfig
        {
            Enabled = true,
            DiscoveryPrefix = "homeassistant",
            BaseTopic = "irrigation_controller",
            PublishIntervalSeconds = 30
        },
        Safety = new SafetyConfig
        {
            TurnOffAllZonesOnStartup = true,
            StopAllKnownZonesOnError = true,
            VerifyZoneStateAfterSwitch = true,
            SwitchRetryCount = 2,
            SwitchRetryDelayMs = 750,
            ManualRunsIgnoreWeather = true,
            MaxZoneMinutes = 60
        },
        Hydraulic = new HydraulicConfig
        {
            AllowParallelZones = false,
            MaxParallelZones = 1,
            PauseBetweenZonesSeconds = 0
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
