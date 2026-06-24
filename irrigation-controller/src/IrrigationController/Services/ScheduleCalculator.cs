using IrrigationController.Models;

namespace IrrigationController.Services;

public static class ScheduleCalculator
{
    public static bool IsScheduledDate(ScheduleConfig schedule, DateOnly date)
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

    public static DateTimeOffset? CalculateNextRun(CycleConfig cycle, DateTimeOffset now, int lookaheadDays = 14)
    {
        if (cycle.Mode != CycleMode.Automatic || !cycle.Enabled || cycle.Schedule is null || cycle.Schedule.Times.Count == 0)
        {
            return null;
        }

        for (var dayOffset = 0; dayOffset <= lookaheadDays; dayOffset++)
        {
            var date = now.Date.AddDays(dayOffset);
            if (!IsScheduledDate(cycle.Schedule, DateOnly.FromDateTime(date)))
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
}
