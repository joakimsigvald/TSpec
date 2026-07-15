using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A TimeOnly source value sequence: starts at the data generator's epoch time of day (noon)
/// and each subsequent value is one hour later, unless configured otherwise with StartingAt and Spaced.
/// TimeOnly arithmetic wraps at midnight, so a sequence is exhausted when it cycles back to a produced value.
/// </summary>
internal class TimeOnlySequence : Sequence<TimeOnly>
{
    protected override TimeOnly DefaultStart => TimeOnly.FromDateTime(PrimitiveStrategy.Epoch);
    protected override TimeOnly DefaultStep(TimeOnly current) => current.AddHours(1);
}
