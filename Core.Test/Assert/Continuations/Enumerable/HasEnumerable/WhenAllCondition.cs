using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenAllCondition : Spec
{
    [Fact]
    public void GivenEmpty_ThenCompletes() 
        => Zero<int>().Has().All(it => it == 1).and.Is().Empty();

    [Fact]
    public void GivenAllSatisfyCondition_ThenCompletes()
    {
        int[] arr = [1, 2];
        arr.Has().All(it => it >= 1).and.Is().not.Empty();
    }

    [Fact]
    public void GivenConditionNotSatisfiedForAll_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(it => it < 3));
        ex.HasMessage("Expected arr to have all it < 3 but found 1 that does not: [3]", "Arr has all it < 3");
    }

    [Fact]
    public void GivenAMethodGroup_ThenItReadsAsTheWordsOfItsName()
    {
        int[] arr = [1, -3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().All(IsPositive));
        ex.HasMessage("Expected arr to have all is positive but found 1 that does not: [-3]", "Arr has all is positive");
    }

    [Fact]
    public void GivenAnIndexedMethodGroup_ThenItReadsAsTheWordsOfItsName()
    {
        int[] arr = [1, 2];
        arr.Has().All(IsAboveItsIndex);
        Specification.Is("Arr has all is above its index");
    }

    private static bool IsPositive(int value) => value > 0;
    private static bool IsAboveItsIndex(int value, int index) => value > index;
}