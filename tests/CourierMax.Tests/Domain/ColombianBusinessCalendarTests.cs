using FluentAssertions;
using CourierMax.Domain.Services;

namespace CourierMax.Tests.Domain;

public class ColombianBusinessCalendarTests
{
    [Fact]
    public void CountBusinessDays_FridayToMonday_CountsOneBusinessDay()
    {
        // RN-02 example: created on a Friday with 1 business-day SLA must be delivered on Monday.
        var friday = new DateTime(2026, 6, 19); // Friday
        var monday = new DateTime(2026, 6, 22); // Monday

        var elapsed = ColombianBusinessCalendar.CountBusinessDays(friday, monday);

        elapsed.Should().Be(1);
    }

    [Fact]
    public void CountBusinessDays_FridayToTuesday_CountsTwoBusinessDays()
    {
        var friday = new DateTime(2026, 6, 19);
        var tuesday = new DateTime(2026, 6, 23);

        var elapsed = ColombianBusinessCalendar.CountBusinessDays(friday, tuesday);

        elapsed.Should().Be(2);
    }

    [Fact]
    public void CountBusinessDays_SkipsColombianHoliday()
    {
        // 2026-06-29 is a Colombian holiday (San Pedro y San Pablo) and falls on a Monday.
        var before = new DateTime(2026, 6, 26); // Friday
        var afterHoliday = new DateTime(2026, 6, 30); // Tuesday

        // Sat(0) Sun(0) Mon-holiday(0) Tue(1)
        var elapsed = ColombianBusinessCalendar.CountBusinessDays(before, afterHoliday);

        elapsed.Should().Be(1);
    }

    [Fact]
    public void IsBusinessDay_Holiday_ReturnsFalse()
    {
        ColombianBusinessCalendar.IsBusinessDay(new DateOnly(2026, 1, 1)).Should().BeFalse();
    }

    [Fact]
    public void IsBusinessDay_Weekend_ReturnsFalse()
    {
        ColombianBusinessCalendar.IsBusinessDay(new DateOnly(2026, 6, 20)).Should().BeFalse(); // Saturday
    }

    [Fact]
    public void IsBusinessDay_RegularWeekday_ReturnsTrue()
    {
        ColombianBusinessCalendar.IsBusinessDay(new DateOnly(2026, 6, 23)).Should().BeTrue(); // Tuesday
    }
}
