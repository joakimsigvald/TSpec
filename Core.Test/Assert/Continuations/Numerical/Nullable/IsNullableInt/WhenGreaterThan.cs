using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Nullable.IsNullableInt;

public class WhenGreaterThan : Spec
{
    [Fact] public void GivenGreater_ThenDoesNotThrow() => ((int?)3).Is().GreaterThan(2);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public void GivenFail_ThenGetException(int? a, int b)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().GreaterThan(b));
        ex.HasMessage($"Expected a to be greater than {b} but found {a}", "A is greater than b");
    }
}