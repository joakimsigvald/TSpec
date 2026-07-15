using TSpec.Assert;

namespace TSpec.Test.Given;

public class WhenGivenList : Spec<List<int>>
{
    [Fact]
    public void ThenReturnList()
    {
        When(_ => [1, 2, 3]).Then().Result.Has().Count(3);
        Specification.Is(
            """
            When [1, 2, 3]
            Then Result has count 3
            """);
    }
}

public class WhenGivenAList : Spec<List<int>>
{
    [Fact]
    public void ThenReturnTheList()
    {
        When(_ => A<List<int>>()).Then().Result.Is(The<List<int>>());
        Specification.Is(
            """
            When a List<int>
            Then Result is the List<int>
            """);
    }
}