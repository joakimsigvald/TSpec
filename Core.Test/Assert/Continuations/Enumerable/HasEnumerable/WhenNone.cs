using TSpec.Assert;
using TSpec.Internal.Specification;
using Xunit.Sdk;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenNone : Spec
{
    [Fact]
    public void GivenEmpty_ThenCompletes()
    {
        Zero<int>().Has().None(it => it > 3);
        Specification.Is("Zero ints has none it > 3");
    }

    [Fact]
    public void GivenSomeSatisfyCondition_ThenThrows()
    {
        int[] arr = [1, 4];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().None(it => it > 3));
        ex.HasMessage("Expected arr to have none it > 3 but found 1 that does: [4]", "Arr has none it > 3");
    }

    [Fact]
    public void GivenSomeSatisfyIndexedCondition_ThenThrows()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<XunitException>(() => arr.Has().None((it, i) => it == i + 1));
        ex.HasMessage("Expected arr to have none (it, i) => it == i + 1 but found 1 that does: [1]", "Arr has none (it, i) => it == i + 1");
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAny_ThenCompletes()
    {
        int[] arr = [2, 3];
        arr.Has().None(it => it == 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenIndexedConditionNotSatisfiedForAny_ThenCompletes()
    {
        int[] arr = [2, 3];
        arr.Has().None((it, i) => it == i + 1).and.Is().not.Empty();
    }
}