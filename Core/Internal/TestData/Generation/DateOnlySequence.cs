using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A DateOnly source value sequence: starts at the data generator's epoch date and each subsequent value
/// is one day later, unless configured otherwise with StartingAt and Spaced.
/// </summary>
internal class DateOnlySequence : Sequence<DateOnly>
{
    protected override DateOnly DefaultStart => DateOnly.FromDateTime(PrimitiveStrategy.Epoch);
    protected override DateOnly DefaultStep(DateOnly current) => current.AddDays(1);
}
