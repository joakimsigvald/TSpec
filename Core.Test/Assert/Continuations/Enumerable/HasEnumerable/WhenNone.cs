using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenNone : Spec
{
    [Fact]
    public void GivenEmpty_ThenDoesNotThrow()
    {
        Zero<int>().Has().None(it => it > 3);
        Specification.Is("Zero int has not some it > 3");
    }

    [Fact]
    public void GivenSomeSatisfyCondition_ThenThrows()
    {
        int[] arr = [1, 4];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().None(it => it > 3));
        ex.HasMessage($"Expected arr to not have some element satisfying the condition but found {arr.ParseValue()}", "Arr has not some it > 3");
    }

    [Fact]
    public void GivenSomeSatisfyIndexedCondition_ThenThrows()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().None((it, i) => it == i + 1));
        ex.HasMessage($"Expected arr to not have some element satisfying the condition but found {arr.ParseValue()}", "Arr has not some (it, i) => it == i + 1");
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAny_ThenDoesNotThrow()
    {
        int[] arr = [2, 3];
        arr.Has().None(it => it == 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenIndexedConditionNotSatisfiedForAny_ThenDoesNotThrow()
    {
        int[] arr = [2, 3];
        arr.Has().None((it, i) => it == i + 1).and.Is().not.Empty();
    }
}