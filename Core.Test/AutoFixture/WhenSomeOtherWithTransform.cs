using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenSomeOtherWithTransform : Spec<MyRetriever, MyModel[]>
{
    public WhenSomeOtherWithTransform()
        => Using(() => SomeOther<MyModel>(m => m with { Id = The<int>() }, An<int>(i => 1 + i % 10)))
        .When(_ => _.List());

    [Fact]
    public void ThenArrayHasElementsWithSetup()
    {
        Result.Has().Count(The<int>());
        Result.Has().All(m => m.Id == The<int>());
        Specification.Is(
            $$$"""
            Using some other MyModel { Id = the int, an int { 1 + i % 10 } }
            When _.List()
            Then Result has count 'the int' = {{{The<int>()}}}
            Result has all m.Id == the int
            """);
    }
}