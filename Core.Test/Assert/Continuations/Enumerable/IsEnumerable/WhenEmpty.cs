using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenEmpty : Spec
{
    [Fact]
    public void GivenEmpty_ThenDoesNotThrow() => Zero<int>().Is().Empty().and.Empty();

    [Fact]
    public void GivenNotEmpty_ThenGetException()
    {
        int[] arr = [1];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Empty());
        ex.HasMessage($"Expected arr to be empty but found [1]", "Arr is empty");
    }
}