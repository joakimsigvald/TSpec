using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenEqualTo : Spec
{
    [Fact]
    public void GivenEqualTo_ThenDoesNotThrow() => (new int[] {1, 2}).Is().EqualTo([1, 2]).and.not.Empty();

    [Fact]
    public void GivenNotEqualTo_ThenGetException()
    {
        int[] arr = [1, 2];
        int[] secondArr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().EqualTo(secondArr));
        ex.HasMessage($"Expected arr to be equal to [1, 3] but found [1, 2]", "Arr is equal to secondArr");
    }
}