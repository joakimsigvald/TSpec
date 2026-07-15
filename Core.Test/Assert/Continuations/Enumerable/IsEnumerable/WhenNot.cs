using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.IsEnumerable;

public class WhenNot : Spec
{
    [Fact]
    public void GivenNotSameInstance_ThenDoesNotThrow() => (new int[] { 1 }).Is().Not([1]).and.not.Empty();

    [Fact]
    public void GivenSameInstance_ThenGetException()
    {
        int[] arr = [1, 2];
        var arrAgain = arr;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Is().Not(arrAgain));
        ex.HasMessage($"Expected arr to not be [1, 2] but found [1, 2]", "Arr is not arrAgain");
    }
}