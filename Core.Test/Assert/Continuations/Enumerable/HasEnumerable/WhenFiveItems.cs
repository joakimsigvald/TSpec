using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenFiveItems : Spec
{
    [Fact]
    public void GivenFiveItems_ThenDoesNotThrow()
    {
        Five<int>().Has().FiveItems().and.Is().not.Empty();
        Specification.Is(
            """
            Five int has five items
                and is not empty
            """);
    }

    [Fact]
    public void GivenFiveItems_AndVerifyIt()
    {
        Five<int>().Has().FiveItems().that.fifth.Is(TheFifth<int>());
        Specification.Is("Five int has five items that fifth is the fifth int");
    }
}