namespace CourierMax.Domain.Services;

public static class ColombianBusinessCalendar
{
    private static readonly HashSet<DateOnly> Holidays2026 = new()
    {
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 1, 26),
        new DateOnly(2026, 1, 30),
        new DateOnly(2026, 3, 24),
        new DateOnly(2026, 5, 1),
        new DateOnly(2026, 6, 1),
        new DateOnly(2026, 6, 29),
        new DateOnly(2026, 7, 20),
        new DateOnly(2026, 8, 17),
        new DateOnly(2026, 10, 20),
        new DateOnly(2026, 11, 9),
        new DateOnly(2026, 12, 8)
    };

    public static bool IsBusinessDay(DateOnly date) =>
        date.DayOfWeek != DayOfWeek.Saturday &&
        date.DayOfWeek != DayOfWeek.Sunday &&
        !Holidays2026.Contains(date);

    /// <summary>
    /// Counts business days strictly after <paramref name="start"/> up to and including <paramref name="end"/>.
    /// </summary>
    public static int CountBusinessDays(DateTime start, DateTime end)
    {
        var startDate = DateOnly.FromDateTime(start);
        var endDate = DateOnly.FromDateTime(end);

        if (endDate <= startDate)
            return 0;

        var count = 0;
        for (var day = startDate.AddDays(1); day <= endDate; day = day.AddDays(1))
        {
            if (IsBusinessDay(day))
                count++;
        }

        return count;
    }

    public static DateTime AddBusinessDays(DateTime start, int businessDays)
    {
        var date = DateOnly.FromDateTime(start);
        var added = 0;
        while (added < businessDays)
        {
            date = date.AddDays(1);
            if (IsBusinessDay(date))
                added++;
        }

        return date.ToDateTime(TimeOnly.MinValue);
    }
}
