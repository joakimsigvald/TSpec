using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenFourItemsCondition : Spec
{
    [Fact]
    public void GivenFourItemsConditionSatisfied_AndVerifyIt_ThenDoesNotThrow()
    {
        int[] arr = [1, 2, 3, 4];
        arr.Has().FourItems(it => it > 0).that.fourth.Is(4);
    }

    [Fact]
    public void GivenFiveSatisfyingItems_ThenGetException()
    {
        int[] arr = [1, 2, 3, 4, 5];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().FourItems(it => it > 0));
        ex.HasMessage($"Expected arr to have four items satisfying the condition but found 5: [1, 2, 3, 4, 5]",
            "Arr has four items it > 0");
    }
}