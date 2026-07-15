namespace TSpec.Assert.Continuations.Numerical.Fractional;

/// <summary>
/// Object that allows assertions to be made on the provided decimal
/// </summary>
public record IsDecimal : IsFractional<decimal, IsDecimal>
{
    private protected override void AssertEqual(decimal expected, decimal actual, decimal precision)
        => Xunit.Assert.True(Math.Abs(actual - expected) <= precision);

    private protected override void AssertNotEqual(decimal expected, decimal actual, decimal precision)
        => Xunit.Assert.False(Math.Abs(actual - expected) <= precision);
}