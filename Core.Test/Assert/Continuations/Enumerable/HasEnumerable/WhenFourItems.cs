using TSpec.Assert;

namespace TSpec.Test.Assert.Continuations.Enumerable.HasEnumerable;

public class WhenFourItems : Spec
{
    [Fact]
    public void GivenFourItems_ThenDoesNotThrow()
    {
        Four<int>().Has().FourItems().and.Is().not.Empty();
        Specification.Is(
            """
            Four int has four items
                and is not empty
            """);
    }

    [Fact]
    public void GivenFourItems_AndVerifyIt()
    {
        Four<int>().Has().FourItems().that.fourth.Is(TheFourth<int>());
        Specification.Is("Four int has four items that fourth is the fourth int");
    }
}