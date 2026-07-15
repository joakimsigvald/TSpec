using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenAllAssert : Spec
{
    [Fact]
    public void GivenEmpty_ThenDoesNotThrow() 
        => Zero<int>().Has().All(it => it.Is(1)).and.Is().Empty();

    [Fact]
    public void GivenAllSatisfyCondition_ThenDoesNotThrow()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => it.Is().not.LessThan(1)).and.Is().not.Empty();
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAll_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => it.Is().LessThan(3)));
        ex.HasMessage(
            """
            Expected arr to have all elements satisfying the assertion but found [1, 3]
            Expected it to be less than 3 but found 3
            """, 
            "Arr has all it.Is().LessThan(3)");
    }
}