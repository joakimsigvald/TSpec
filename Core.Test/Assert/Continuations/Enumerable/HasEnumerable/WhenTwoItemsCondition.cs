using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenTwoItemsCondition : Spec
{
    [Fact]
    public void GivenTwoItemsConditionSatisfied_ThenDoesNotThrow()
    {
        int[] arr = [1, 1];
        arr.Has().TwoItems(it => it == 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenTwoSatisfyingElementOutOfThree_ThenDoesNotThrow()
    {
        int[] arr = [1, 1, 999];
        arr.Has().TwoItems(it => it == 1);
    }

    [Fact]
    public void GivenTwoItemsConditionSatisfied_AndVerifyIt_ThenDoesNotThrow()
    {
        int[] arr = [1, 1];
        arr.Has().TwoItems(it => it == 1).that.first.Is().LessThan(200);
    }

    [Fact]
    public void GivenThreeSatisfyingItems_ThenGetException()
    {
        int[] arr = [1, 2, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().TwoItems(it => it > 0));
        ex.HasMessage($"Expected arr to have two items satisfying the condition but found 3: [1, 2, 3]",
            "Arr has two items it > 0");
    }
}