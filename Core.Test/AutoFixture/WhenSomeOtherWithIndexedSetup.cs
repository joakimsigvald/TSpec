using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenSomeOtherWithIndexedSetup : Spec<MyRetriever, MyModel[]>
{
    public WhenSomeOtherWithIndexedSetup()
        => Using(() => SomeOther<MyModel>((m, i) => m.Id = i, An<int>(i => 1 + i % 10)))
        .When(_ => _.List());

    [Fact]
    public void ThenArrayHasElementsWithSetup()
    {
        Result.Has().Count(The<int>());
        Result.Has().All((m, i) => m.Id == i);
        Specification.Is(
            $$$"""
            Using some other MyModel { Id = i, an int { 1 + i % 10 } }
            When _.List()
            Then Result has count 'the int' = {{{The<int>()}}}
            Result has all (m, i) => m.Id == i
            """);
    }
}