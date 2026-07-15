using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenNotEqualTo : Spec
{
    [Fact]
    public void GivenNotEqualTo_ThenDoesNotThrow() => (new int[] { 1 }).Is().not.EqualTo([2]).and.not.Empty();

    [Fact]
    public void GivenEqualTo_ThenGetException()
    {
        int[] arr = [1, 2];
        int[] secondArr = [1, 2];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().not.EqualTo(secondArr));
        ex.HasMessage($"Expected arr to not be equal to [1, 2] but found [1, 2]", "Arr is not equal to secondArr");
    }
}