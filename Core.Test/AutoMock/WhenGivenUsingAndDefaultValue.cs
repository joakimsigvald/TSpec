using TSpec.Assert;
using TSpec.Test.AutoFixture;

namespace TSpec.Test.AutoMock;

public class WhenGivenUsingAndDefaultValue : Spec<MyWrapper<string>, (string, string)>
{
    [Fact]
    public void ThenApplyBothAsValues()
    {
        Using("ABC", For.Input).And("DEF", For.Subject)
            .When(_ => _.GetValues(A<string>()))
            .Then().Result.Is(("DEF", "ABC"));
        Specification.Is(
            """
            Using "ABC" for Input
              and "DEF" for Subject
            When _.GetValues(a string)
            Then Result is ("DEF", "ABC")
            """);
    }

    [Fact]
    public void ThenApplyBothAsLambdas()
    {
        Using(() => "ABC").And(() => "DEF", For.Input)
            .When(_ => _.GetValues(A<string>()))
            .Then().Result.Is(("ABC", "DEF"));
        Specification.Is(
            """
            Using "ABC"
              and "DEF" for Input
            When _.GetValues(a string)
            Then Result is ("ABC", "DEF")
            """);
    }

    [Fact]
    public void UsingTag_ThenApplyBothAsLambdas()
    {
        Tag<string> def = new();
            Using(() => "ABC", For.Input)
            .And(def, For.Subject)
            .Given(def).Is("DEF")
            .When(_ => _.GetValues(A<string>()))
            .Then().Result.Is(("DEF", "ABC"));
        Specification.Is(
            """
            Using "ABC" for Input
              and def for Subject
            Given def is "DEF"
            When _.GetValues(a string)
            Then Result is ("DEF", "ABC")
            """);
    }
}

public class WhenGivenUsingAndDefaultModel : Spec<MyWrapper<MyModel>, (MyModel, MyModel)>
{
    private readonly MyModel _first = new() { Id = 1};
    private readonly MyModel _second = new() { Id = 2 };

    [Fact]
    public void ThenApplyBothAsValues()
    {
        Using(_first, For.Input).And(_second, For.Subject)
            .When(_ => _.GetValues(A<MyModel>()))
            .Then().Result.Is((_second, _first));
        Specification.Is(
            """
            Using _first for Input
              and _second for Subject
            When _.GetValues(a MyModel)
            Then Result is (_second, _first)
            """);
    }

    [Fact]
    public void GivenTagsThenApplyBothAsValues()
    {
        Tag<MyModel> one = new(), two = new();
        Using(one, For.Input).And(two, For.Subject)
            .Given(one).Is(_first).And(two).Is(_second)
            .When(_ => _.GetValues(A<MyModel>()))
            .Then().Result.Is((_second, _first));
        Specification.Is(
            """
            Using one for Input
              and two for Subject
            Given two is _second
              and one is _first
            When _.GetValues(a MyModel)
            Then Result is (_second, _first)
            """);
    }

    [Fact]
    public void ThenApplyBothAsLambdas()
    {
        Using(() => _first, For.Input).And(() => _second, For.Subject)
            .When(_ => _.GetValues(A<MyModel>()))
            .Then().Result.Is((_second, _first));
        Specification.Is(
            """
            Using _first for Input
              and _second for Subject
            When _.GetValues(a MyModel)
            Then Result is (_second, _first)
            """);
    }
}