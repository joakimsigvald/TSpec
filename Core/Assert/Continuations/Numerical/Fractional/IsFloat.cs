namespace TSpec.Assert.Continuations.Numerical.Fractional;

/// <summary>
/// Object that allows assertions to be made on the provided float
/// </summary>
public record IsFloat : IsFractional<float, IsFloat>
{
    private protected override void AssertEqual(float expected, float actual, float precision)
        => Xunit.Assert.Equal(expected, actual, precision);

    private protected override void AssertNotEqual(float expected, float actual, float precision)
        => Xunit.Assert.NotEqual(expected, actual, precision);
}