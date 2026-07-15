using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Fractional.IsDecimal;

public class WhenNotAround : Spec
{
    [Theory]
    [InlineData(1, 2, 0)]
    [InlineData(1, 3, 1.99)]
    public void GivenNotAround_ThenDoesNotThrow(decimal a, decimal b, decimal precision)
        => a.Is().not.Around(b, precision);

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(2, 3, 1)]
    public void GivenFail_ThenGetException(decimal a, decimal b, decimal precision)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.Around(b, precision));
        ex.HasMessage($"Expected a to not be around {b} but found {a}", "A is not around b");
    }
}