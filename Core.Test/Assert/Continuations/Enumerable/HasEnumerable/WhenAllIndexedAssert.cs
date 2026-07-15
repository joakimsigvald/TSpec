using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenAllIndexedAssert : Spec
{
    [Fact]
    public void GivenEmpty_ThenDoesNotThrow() 
        => Zero<int>().Has().All((it, i) => it.Is(i)).and.Is().Empty();

    [Fact]
    public void GivenAllSatisfyCondition_ThenDoesNotThrow()
    {
        int[] arr = [1, 2];
        arr.Has().All((it, i) => it.Is(i + 1)).and.Is().not.Empty();
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAll_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All((it, i) => it.Is(i + 1)));
        ex.HasMessage(
            """
            Expected arr to have all elements satisfying the assertion but found [1, 3]
            Expected it to be 2 but found 3
            """,
            "Arr has all (it, i) => it.Is(i + 1)");
    }
}