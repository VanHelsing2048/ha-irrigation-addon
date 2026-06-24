using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class IrrigationOverviewService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly IrrigationConfigValidator _validator;
    private readonly HomeAssistantClient _homeAssistant;
    private readonly CycleRunner _runner;

    public IrrigationOverviewService(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        IrrigationConfigValidator validator,
        HomeAssistantClient homeAssistant,
        CycleRunner runner)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _validator = validator;
        _homeAssistant = homeAssistant;
        _runner = runner;
    }

    public async Task<IrrigationOverview> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var state = await _stateStore.GetAsync(cancellationToken);
        var validation = _validator.Validate(config);

        return new IrrigationOverview
        {
            Runner = _runner.Current,
            ExpectedEndText = FormatDateTime(_runner.Current.ExpectedEndAt),
            Validation = validation,
            LastWaterBalanceUpdateDate = state.LastWaterBalanceUpdateDate ?? "-",
            Diagnostics = state.Diagnostics,
            Weather = await BuildWeatherOverviewAsync(config, cancellationToken),
            RecentEvents = state.Events.Take(80).ToList(),
            Cycles = BuildCycleOverview(config),
            Zones = await BuildZoneOverviewAsync(config, state, cancellationToken)
        };
    }

    private static List<CycleOverview> BuildCycleOverview(IrrigationConfig config)
    {
        return config.Cycles
            .Select(cycle =>
            {
                var nextRun = CalculateNextRun(cycle.Value);
                return new CycleOverview
                {
                    Id = cycle.Key,
                    Name = cycle.Value.Name,
                    Mode = cycle.Value.Mode,
                    Enabled = cycle.Value.Enabled,
                    NextRun = nextRun,
                    NextRunText = cycle.Value.Enabled ? FormatDateTime(nextRun) : "Disabilitato"
                };
            })
            .OrderBy(cycle => cycle.NextRun ?? DateTimeOffset.MaxValue)
            .ThenBy(cycle => cycle.Name)
            .ToList();
    }

    private async Task<WeatherOverview> BuildWeatherOverviewAsync(
        IrrigationConfig config,
        CancellationToken cancellationToken)
    {
        return new WeatherOverview
        {
            Entity = config.Weather.Entity,
            ForecastType = config.Weather.ForecastType,
            State = await ReadStateSafeAsync(config.Weather.Entity, cancellationToken) ?? "unknown"
        };
    }

    private async Task<List<ZoneOverview>> BuildZoneOverviewAsync(
        IrrigationConfig config,
        IrrigationRuntimeState state,
        CancellationToken cancellationToken)
    {
        var zones = new List<ZoneOverview>();

        foreach (var (zoneId, zone) in config.Zones.OrderBy(item => item.Value.Name))
        {
            var homeAssistantState = await ReadStateSafeAsync(zone.Entity, cancellationToken);
            state.WaterBalance.TryGetValue(zoneId, out var balance);
            state.Calibrations.TryGetValue(zoneId, out var calibration);

            zones.Add(new ZoneOverview
            {
                Id = zoneId,
                Name = zone.Name,
                Entity = zone.Entity,
                State = homeAssistantState ?? "unknown",
                StateClass = ToStateClass(homeAssistantState),
                WaterBalanceMm = balance,
                CalibratedPrecipitationRateMmH = calibration?.PrecipitationRateMmH,
                CalibrationText = calibration is null
                    ? "-"
                    : $"{calibration.PrecipitationRateMmH:0.##} mm/h ({calibration.DistributionUniformityPercent:0.#}%)"
            });
        }

        return zones;
    }

    private async Task<string?> ReadStateSafeAsync(string entityId, CancellationToken cancellationToken)
    {
        try
        {
            return await _homeAssistant.GetStateAsync(entityId, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static DateTimeOffset? CalculateNextRun(CycleConfig cycle)
    {
        if (cycle.Mode != CycleMode.Automatic || !cycle.Enabled || cycle.Schedule is null || cycle.Schedule.Times.Count == 0)
        {
            return null;
        }

        var now = DateTimeOffset.Now;
        for (var dayOffset = 0; dayOffset <= 14; dayOffset++)
        {
            var date = now.Date.AddDays(dayOffset);
            if (cycle.Schedule.Days.Count > 0 && !cycle.Schedule.Days.Contains(date.DayOfWeek))
            {
                continue;
            }

            foreach (var configuredTime in cycle.Schedule.Times)
            {
                if (!TimeOnly.TryParse(configuredTime, out var time))
                {
                    continue;
                }

                var candidate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0, now.Offset);
                if (candidate > now)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string FormatDateTime(DateTimeOffset? value)
    {
        if (value is null)
        {
            return "-";
        }

        var local = value.Value.ToLocalTime();
        return local.Date == DateTimeOffset.Now.Date
            ? local.ToString("HH:mm")
            : local.ToString("dd/MM HH:mm");
    }

    private static string ToStateClass(string? state) =>
        state?.ToLowerInvariant() switch
        {
            "on" => "on",
            "off" => "off",
            _ => "unknown"
        };
}
