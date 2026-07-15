using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.IsInt;

public class WhenNotLessThan : Spec
{
    [Theory]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void GivenNotLess_ThenDoesNotThrow(int a, int b) => a.Is().not.LessThan(b);

    [Theory]
    [InlineData(1, 2)]
    public void GivenFail_ThenGetException(int a, int b)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().not.LessThan(b));
        ex.HasMessage($"Expected a to not be less than {b} but found {a}", "A is not less than b");
    }
}