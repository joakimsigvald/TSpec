using TSpec.Assert;

namespace TSpec.Test.Given;

public class WhenGivenUniqueInt : Spec<int[]>
{
    [Fact]
    public void ThenGenerateUniqueIntArray()
    {
        int range = 10;
        When(_ => Five<int>()).Using<int>(i => i % range)
            .Then().Result.Is().Distinct()
            .and.Has().All(i => i >= 0 && i < range);
        Specification.Is(
            """
            Using int is i % range
            When five ints
            Then Result is distinct
                and has all i >= 0 && i < range
            """);
    }

    [Fact]
    public void ThenGenerateUniqueIntValues()
    {
        int range = 10;
        When(_ => Five<int>(i => i % range))
            .Then().Result.Is().Distinct()
            .and.Has().All(i => i >= 0 && i < range);
        Specification.Is(
            """
            When five ints { i % range }
            Then Result is distinct
                and has all i >= 0 && i < range
            """);
    }

    [Fact]
    public void ThenGenerateUniqueOtherIntValues()
    {
        int range = 10;
        When(_ => [
            Any<int>(),
            Any<int>(),
            Any<int>(),
            Any<int>(),
            Any<int>()])
            .Using<int>(i => i % range)
            .Then().Result.Is().Distinct()
            .and.Has().All(i => i >= 0 && i < range);
        Specification.Is(
            """
            Using int is i % range
            When [any int, any int, any int, any int, any int]
            Then Result is distinct
                and has all i >= 0 && i < range
            """);
    }
}