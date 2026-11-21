namespace MyHotel.Host.Spec;

/// <summary>
/// The hotel's clock, stopped at noon in UTC so today is the same day wherever the suite runs. It
/// starts on the specs' Today, the day their stays are named from.
/// </summary>
internal sealed class Clock : TimeProvider
{
    public DateOnly Today { get; set; } = Core.Spec.Time.Today;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(Today, new TimeOnly(12, 0), TimeSpan.Zero);
}
