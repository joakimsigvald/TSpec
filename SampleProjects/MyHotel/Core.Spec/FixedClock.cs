namespace MyHotel.Core.Spec;

/// <summary>
/// A clock stopped at noon on one day, in UTC, so today is that day wherever the suite runs.
/// </summary>
internal sealed class FixedClock(DateOnly today) : TimeProvider
{
    public static TimeProvider TodayIs(DateOnly today) => new FixedClock(today);

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(today, new TimeOnly(12, 0), TimeSpan.Zero);
}
