namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A TimeSpan source value sequence: starts at one day and each subsequent value is one day longer,
/// unless configured otherwise with StartingAt and Spaced.
/// </summary>
internal class TimeSpanSequence : Sequence<TimeSpan>
{
    protected override TimeSpan DefaultStart => TimeSpan.FromDays(1);
    protected override TimeSpan DefaultStep(TimeSpan current) => current + TimeSpan.FromDays(1);
}
