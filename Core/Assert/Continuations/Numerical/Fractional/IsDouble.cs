namespace TSpec.Assert.Continuations.Numerical.Fractional;

/// <summary>
/// Object that allows assertions to be made on the provided double
/// </summary>
public record IsDouble : IsFractional<double, IsDouble>
{
    private protected override void AssertEqual(double expected, double actual, double tolerance)
        => Xunit.Assert.Equal(expected, actual, tolerance);

    private protected override void AssertNotEqual(double expected, double actual, double tolerance)
        => Xunit.Assert.NotEqual(expected, actual, tolerance);
}