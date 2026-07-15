using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenOneItem : Spec
{
    [Fact]
    public void GivenOneItem_ThenDoesNotThrow()
    {
        One<int>().Has().OneItem().and.Is().not.Empty();
        Specification.Is(
            """
            One int has one item
                and is not empty
            """);
    }

    [Fact]
    public void GivenOneItem_AndVerifyIt()
    {
        One<int>().Has().OneItem().that.Is(The<int>());
        Specification.Is("One int has one item that is the int");
    }

    [Fact]
    public void GivenEmpty_ThenGetException()
    {
        int[] arr = Zero<int>();
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().OneItem().that.Is(123));
        ex.HasMessage($"Expected arr to have one item but found 0: []", "Arr has one item");
    }

    [Fact]
    public void GivenTwoElements_ThenGetException()
    {
        int[] arr = [1, 3];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => arr.Has().OneItem().that.Is(123));
        ex.HasMessage($"Expected arr to have one item but found 2: [1, 3]", "Arr has one item");
    }
}