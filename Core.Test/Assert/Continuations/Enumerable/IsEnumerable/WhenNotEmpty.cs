using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenNotEmpty : Spec
{
    [Fact]
    public void GivenNotEmpty_ThenDoesNotThrow() => One<int>().Is().not.Empty().and.Has().OneItem();

    [Fact]
    public void GivenEmpty_ThenGetException()
    {
        var arr = Zero<int>();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().not.Empty());
        ex.HasMessage($"Expected arr to not be empty but found []", "Arr is not empty");
    }
}