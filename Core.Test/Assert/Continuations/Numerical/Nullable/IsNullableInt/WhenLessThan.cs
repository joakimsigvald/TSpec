using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Numerical.Nullable.IsNullableInt;

public class WhenLessThan : Spec
{
    [Fact] public void GivenLess_ThenDoesNotThrow() => ((int?)1).Is().LessThan(2);

    [Theory]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void GivenFail_ThenGetException(int? a, int b)
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => a.Is().LessThan(b));
        ex.HasMessage($"Expected a to be less than {b} but found {a}", "A is less than b");
    }
}