using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenThreeItems : Spec
{
    [Fact]
    public void GivenThreeItems_ThenDoesNotThrow()
    {
        Three<int>().Has().ThreeItems().and.Is().not.Empty();
        Specification.Is(
            """
            Three int has three items
                and is not empty
            """);
    }

    [Fact]
    public void GivenThreeItems_AndVerifyIt()
    {
        var (first, second, third) = Three<int>().Has().ThreeItems().that;
        first.Is(TheFirst<int>());
        second.Is(TheSecond<int>());
        third.Is(TheThird<int>());
        Specification.Is("""
            Three int has three items that first is the first int
            Second is the second int
            Third is the third int
            """);
    }

    [Fact]
    public void GivenFourElements_ThenGetException()
    {
        int[] arr = [1, 2, 3, 4];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().ThreeItems());
        ex.HasMessage($"Expected arr to have three items but found 4: [1, 2, 3, 4]",
            "Arr has three items");
    }
}