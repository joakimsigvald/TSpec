using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenSome : Spec
{
    [Fact]
    public void GivenEmpty_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Zero<int>().Has().Some(it => it > 3));
        ex.HasMessage($"Expected zero int to have some element satisfying the condition but found []", "Zero int has some it > 3");
    }

    [Fact]
    public void GivenSomeSatisfyCondition_ThenDoesNotThrow()
    {
        int[] arr = [1, 2];
        arr.Has().Some((it, i) => it == 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenSomeSatisfyIndexedCondition_ThenDoesNotThrow()
    {
        int[] arr = [1, 3];
        arr.Has().Some((it, i) => it == i + 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAny_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().Some(it => it > 3));
        ex.HasMessage($"Expected arr to have some element satisfying the condition but found [1, 3]", "Arr has some it > 3");
    }

    [Fact]
    public void GivenIndexedConditionNotSatisfiedForAny_ThenGetException()
    {
        int[] arr = [2, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().Some((it, i) => it == i + 1));
        ex.HasMessage($"Expected arr to have some element satisfying the condition but found [2, 3]",
            "Arr has some (it, i) => it == i + 1");
    }
}
