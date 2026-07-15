using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Nullable.IsNullableInt;

public class WhenNotGreaterThan : Spec
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public void GivenNotGreater_ThenDoesNotThrow(int? a, int b) => a.Is().not.GreaterThan(b);

    [Theory]
    [InlineData(3, 2)]
    public void GivenFail_ThenGetException(int? a, int b)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.GreaterThan(b));
        ex.HasMessage($"Expected a to not be greater than {b} but found {a}", "A is not greater than b");
    }
}