using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenArrayOfValues : Spec<MyService, IEnumerable<int>>
{
    [Fact]
    public void ThenCanUseTwoValuesGivenSeparatelyFromMock()
    {
        When(_ => _.GetIds()).Using(Two<int>()).Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Using two int
            When _.GetIds()
            Then Result is two int
            """);
    }

    [Fact]
    public void ThenDoNotUseTwoValuesGivenInDifferentSetup()
    {
        When(_ => _.GetIds()).Given<MyModel>(_ => _.Values = Two<int>()).Then().Result.Is().Empty();
        Specification.Is(
            """
            Given MyModel has Values = two int
            When _.GetIds()
            Then Result is empty
            """);
    }

    [Fact]
    public void WhenGetEnumerableOnModel_GivenTwoValues_ThenReturnTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Given<MyModel>(_ => _.Values = Two<int>())
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Given MyModel has Values = two int
            When _.GetModel().Values
            Then Result is two int
            """);
    }

    [Fact]
    public void GivenModelHasSomeValues_AndGivenOneValue_ThenModelHasOneValue()
    {
        When(_ => _.GetModel().Values)
            .Given<MyModel>(_ => _.Values = Some<int>())
            .Using(One<int>())
            .Then().Result.Is(One<int>());
        Specification.Is(
            """
            Given MyModel has Values = some int
            Using one int
            When _.GetModel().Values
            Then Result is one int
            """);
    }

    [Fact]
    public void GivenModelHasAnyNoValues_AndGivenTwoValues_ThenModelHasTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Given<MyModel>(_ => _.Values = AnyNumberOf<int>())
            .Using(Two<int>())
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Given MyModel has Values = any number of int
            Using two int
            When _.GetModel().Values
            Then Result is two int
            """);
    }

    [Fact]
    public void GivenModelHasSomeValues_AndGivenZeroValues_ThenModelHasTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Given<MyModel>(_ => _.Values = Some<int>())
            .Using(Zero<int>())
            .Then().Result.Has().Count(2);
        Specification.Is(
            """
            Given MyModel has Values = some int
            Using zero int
            When _.GetModel().Values
            Then Result has count 2
            """);
    }

    [Fact]
    public void GivenModelHasManyValues_AndGivenZeroValues_ThenModelHasThreeValues()
    {
        When(_ => _.GetModel().Values)
            .Given<MyModel>(_ => _.Values = Many<int>())
            .Using(Zero<int>())
            .Then().Result.Has().Count(3);
        Specification.Is(
            """
            Given MyModel has Values = many int
            Using zero int
            When _.GetModel().Values
            Then Result has count 3
            """);
    }
}