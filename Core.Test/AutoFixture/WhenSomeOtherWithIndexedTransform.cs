using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class WhenSomeOtherWithIndexedTransform : Spec<MyRetriever, MyModel[]>
{
    public WhenSomeOtherWithIndexedTransform()
        => Using(() => SomeOther<MyModel>((m, i) => m with { Id = i }, An<int>(i => 1 + i % 10)))
        .When(_ => _.List());

    [Fact]
    public void ThenArrayHasElementsWithSetup()
    {
        Result.Has().Count(The<int>());
        Result.Has().All((m, i) => m.Id == i);
        Specification.Is(
            $$$"""
            Using some other MyModel { (m, i) => m with { Id = i }, an int { 1 + i % 10 } }
            When _.List()
            Then Result has count 'the int' = {{{The<int>()}}}
            Result has all (m, i) => m.Id == i
            """);
    }
}