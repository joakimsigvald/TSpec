namespace MyHotel.Host.Spec;

/// <summary>
/// The hotel's clock, stopped at noon in UTC so today is the same day wherever the suite runs. It
/// starts on 1 August 2026, before every stay the specs book, so their bookings are open unless a
/// spec moves today past them.
/// </summary>
internal sealed class Clock : TimeProvider
{
    public DateOnly Today { get; set; } = new(2026, 8, 1);
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(Today, new TimeOnly(12, 0), TimeSpan.Zero);
}
