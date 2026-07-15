using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenOneItemCondition : Spec
{
    [Fact]
    public void GivenOneItemConditionSatisfied_ThenDoesNotThrow()
    {
        int[] arr = [123];
        arr.Has().OneItem(it => it == 123).and.Is().not.Empty();
    }

    [Fact]
    public void GivenOneSatisfyingElementOutOfTwo_ThenDoesNotThrow()
    {
        int[] arr = [123, 999];
        arr.Has().OneItem(it => it == 123);
    }

    [Fact]
    public void GivenOneItemConditionSatisfied_AndVerifyIt_ThenDoesNotThrow()
    {
        int[] arr = [123];
        arr.Has().OneItem(it => it == 123).that.Is().LessThan(200);
    }

    [Fact]
    public void GivenOneSatisfyingItemOutOfTwo_AndVerifyIt_ThenDoesNotThrow()
    {
        int[] arr = [123, 999];
        arr.Has().OneItem(it => it == 123).that.Is().GreaterThan(100);
    }

    [Fact]
    public void GivenOneITemConditionNotSatisfied_ThenGetException()
    {
        int[] arr = [999];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().OneItem(it => it == 123));
        ex.HasMessage($"Expected arr to have one item satisfying the condition but found 1: [999]",
            "Arr has one item it == 123");
    }

    [Fact]
    public void GivenEmpty_ThenGetException()
    {
        int[] arr = Zero<int>();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().OneItem(it => it == 123));
        ex.HasMessage($"Expected arr to have one item satisfying the condition but found 0: []",
            "Arr has one item it == 123");
    }

    [Fact]
    public void GivenTwoSatisfyingItems_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().OneItem(it => it > 0));
        ex.HasMessage($"Expected arr to have one item satisfying the condition but found 2: [1, 3]",
            "Arr has one item it > 0");
    }
}