using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingTwoInts : Spec<MyListService, List<int>>
{
    [Fact]
    public void ThenReturnTwoInts()
    {
        Using(Two<int>().ToList()).When(_ => _.List).Then().Result.Has().Count(2);
        Specification.Is(
            """
            Using two ints' ToList()
            When List
            Then Result has count 2
            """);
    }
}