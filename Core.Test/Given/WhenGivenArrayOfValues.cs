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
            Using two ints
            When GetIds()
            Then Result is two ints
            """);
    }

    [Fact]
    public void ThenDoNotUseTwoValuesGivenInDifferentSetup()
    {
        When(_ => _.GetIds()).Using<MyModel>(_ => _.Values = Two<int>()).Then().Result.Is().Empty();
        Specification.Is(
            """
            Using MyModel with Values = two ints
            When GetIds()
            Then Result is empty
            """);
    }

    [Fact]
    public void WhenGetEnumerableOnModel_GivenTwoValues_ThenReturnTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Using<MyModel>(_ => _.Values = Two<int>())
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Using MyModel with Values = two ints
            When GetModel().Values
            Then Result is two ints
            """);
    }

    [Fact]
    public void GivenModelHasSomeValues_AndGivenOneValue_ThenModelHasOneValue()
    {
        When(_ => _.GetModel().Values)
            .Using<MyModel>(_ => _.Values = Some<int>())
            .And(One<int>())
            .Then().Result.Is(One<int>());
        Specification.Is(
            """
            Using MyModel with Values = some ints
              and one int
            When GetModel().Values
            Then Result is one int
            """);
    }

    [Fact]
    public void GivenModelHasAnyNoValues_AndGivenTwoValues_ThenModelHasTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Using<MyModel>(_ => _.Values = AnyNumberOf<int>())
            .And(Two<int>())
            .Then().Result.Is(Two<int>());
        Specification.Is(
            """
            Using MyModel with Values = any number of ints
              and two ints
            When GetModel().Values
            Then Result is two ints
            """);
    }

    [Fact]
    public void GivenModelHasSomeValues_AndGivenZeroValues_ThenModelHasTwoValues()
    {
        When(_ => _.GetModel().Values)
            .Using<MyModel>(_ => _.Values = Some<int>())
            .And(Zero<int>())
            .Then().Result.Has().Count(2);
        Specification.Is(
            """
            Using MyModel with Values = some ints
              and zero ints
            When GetModel().Values
            Then Result has count 2
            """);
    }

    [Fact]
    public void GivenModelHasManyValues_AndGivenZeroValues_ThenModelHasThreeValues()
    {
        When(_ => _.GetModel().Values)
            .Using<MyModel>(_ => _.Values = Many<int>())
            .And(Zero<int>())
            .Then().Result.Has().Count(3);
        Specification.Is(
            """
            Using MyModel with Values = many ints
              and zero ints
            When GetModel().Values
            Then Result has count 3
            """);
    }
}