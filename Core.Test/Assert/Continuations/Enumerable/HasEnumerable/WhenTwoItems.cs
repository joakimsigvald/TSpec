using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenTwoItems : Spec
{
    [Fact]
    public void GivenTwoItems_ThenDoesNotThrow()
    {
        Two<int>().Has().TwoItems().and.Is().not.Empty();
        Specification.Is(
            """
            Two int has two items
                and is not empty
            """);
    }

    [Fact]
    public void GivenTwoItems_AndVerifyIt()
    {
        var (first, second) = Two<int>().Has().TwoItems().that;
        first.Is(TheFirst<int>());
        second.Is(TheSecond<int>());
        Specification.Is("""
            Two int has two items that first is the first int
            Second is the second int
            """);
    }

    [Fact]
    public void GivenEmpty_ThenGetException()
    {
        int[] arr = Zero<int>();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().TwoItems());
        ex.HasMessage($"Expected arr to have two items but found 0: []", "Arr has two items");
    }

    [Fact]
    public void GivenThreeElements_ThenGetException()
    {
        int[] arr = [1, 2, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().TwoItems());
        ex.HasMessage($"Expected arr to have two items but found 3: [1, 2, 3]", "Arr has two items");
    }
}