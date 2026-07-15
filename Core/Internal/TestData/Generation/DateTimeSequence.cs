using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A DateTime source value sequence: starts at the data generator's epoch and each subsequent value
/// is one day later, unless configured otherwise with StartingAt and Spaced.
/// </summary>
internal class DateTimeSequence : Sequence<DateTime>
{
    protected override DateTime DefaultStart => PrimitiveStrategy.Epoch;
    protected override DateTime DefaultStep(DateTime current) => current.AddDays(1);
}
