using System.Numerics;

namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A numeric source value sequence: starts at one and each subsequent value is one greater,
/// unless configured otherwise with StartingAt and Spaced.
/// Fixed spacings wrap around at the type's boundaries.
/// </summary>
internal class NumericSequence<TSource> : Sequence<TSource> where TSource : INumber<TSource>
{
    protected override TSource DefaultStart => TSource.One;
    protected override TSource DefaultStep(TSource current) => current + TSource.One;
}
