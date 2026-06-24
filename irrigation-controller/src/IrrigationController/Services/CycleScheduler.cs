using IrrigationController.Models;

namespace IrrigationController.Services;

public sealed class CycleScheduler : BackgroundService
{
    private readonly IrrigationConfigStore _configStore;
    private readonly IrrigationStateStore _stateStore;
    private readonly CycleRunner _runner;
    private readonly ILogger<CycleScheduler> _logger;

    public CycleScheduler(
        IrrigationConfigStore configStore,
        IrrigationStateStore stateStore,
        CycleRunner runner,
        ILogger<CycleScheduler> logger)
    {
        _configStore = configStore;
        _stateStore = stateStore;
        _runner = runner;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSchedulesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler check failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task CheckSchedulesAsync(CancellationToken cancellationToken)
    {
        var config = await _configStore.GetAsync(cancellationToken);
        var state = await _stateStore.GetAsync(cancellationToken);
        var now = DateTimeOffset.Now;

        foreach (var (cycleId, cycle) in config.Cycles)
        {
            if (!cycle.Enabled || cycle.Mode != CycleMode.Automatic || cycle.Schedule is null)
            {
                continue;
            }

            if (!IsScheduledDate(cycle.Schedule, DateOnly.FromDateTime(now.Date)))
            {
                continue;
            }

            foreach (var configuredTime in cycle.Schedule.Times)
            {
                if (!TimeOnly.TryParse(configuredTime, out var scheduleTime))
                {
                    continue;
                }

                var scheduledAt = new DateTimeOffset(now.Year, now.Month, now.Day, scheduleTime.Hour, scheduleTime.Minute, 0, now.Offset);
                if (now < scheduledAt || now > scheduledAt.AddMinutes(1))
                {
                    continue;
                }

                var runKey = $"{now:yyyy-MM-dd} {configuredTime}";
                if (state.LastScheduledRuns.TryGetValue(cycleId, out var lastRun) && lastRun == runKey)
                {
                    continue;
                }

                var result = await _runner.StartCycleAsync(cycleId, TriggerSource.Schedule, cancellationToken);
                if (result.Success)
                {
                    state.LastScheduledRuns[cycleId] = runKey;
                    await _stateStore.SaveAsync(state, cancellationToken);
                }
            }
        }
    }

    private static bool IsScheduledDate(ScheduleConfig schedule, DateOnly date)
    {
        if (!string.IsNullOrWhiteSpace(schedule.StartDate) || schedule.EveryDays is not null)
        {
            if (!DateOnly.TryParse(schedule.StartDate, out var startDate) || schedule.EveryDays is null or < 1)
            {
                return false;
            }

            var days = date.DayNumber - startDate.DayNumber;
            return days >= 0 && days % schedule.EveryDays.Value == 0;
        }

        return schedule.Days.Count == 0 || schedule.Days.Contains(date.DayOfWeek);
    }
}
