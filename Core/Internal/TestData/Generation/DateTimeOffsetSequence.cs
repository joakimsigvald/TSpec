using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A DateTimeOffset source value sequence: starts at the data generator's epoch and each subsequent value
/// is one day later, unless configured otherwise with StartingAt and Spaced.
/// </summary>
internal class DateTimeOffsetSequence : Sequence<DateTimeOffset>
{
    protected override DateTimeOffset DefaultStart => new(PrimitiveStrategy.Epoch);
    protected override DateTimeOffset DefaultStep(DateTimeOffset current) => current.AddDays(1);
}
