using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Fractional.IsFloat;

public class WhenAround : Spec
{
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(2, 3, 1)]
    public void GivenAround_ThenDoesNotThrow(float a, float b, float precision)
        => a.Is().Around(b, precision);

    [Theory]
    [InlineData(1, 2, 0)]
    [InlineData(1, 3, 1.99)]
    public void GivenFail_ThenGetException(float a, float b, float precision)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().Around(b, precision));
        ex.HasMessage($"Expected a to be around {b} but found {a}", "A is around b");
    }
}