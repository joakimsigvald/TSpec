using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.AutoFixture;

public class GivenTwoElements : WhenList
{
    public GivenTwoElements() => Given().Two<MyModel>();

    [Fact]
    public void ThenArrayHasTwoElements()
    {
        Then().Result.Has().Count(2);
        Specification.Is(
@"Given two MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has count 2");
    }

    [Fact]
    public void AndExpectingThree_ThenGetException()
    {
        var ex = Xunit.Assert.Throws<XunitException>(() => Then().Result.Has().Count(3));
        Xunit.Assert.StartsWith($"Expected Result to have count 3 but found 2: [", ex.Message);
    }

    [Fact]
    public void ThenTheElementsAreDifferent()
    {
        Then().Result.First().Is().Not(Result.Last());
        Specification.Is(
@"Given two MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.First() is not Result.Last()");
    }

    [Fact]
    public void ThenSecondElementCanBeRetrievedSeparately()
    {
        Then().Result.Last().Is(TheSecond<MyModel>());
        Specification.Is(
@"Given two MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Last() is the second MyModel");
    }
}

public class GivenTwoElementsWithSetup : WhenList
{
    public GivenTwoElementsWithSetup()
        => Given().Two<MyModel>(_ => _.Name = A<string>());

    [Fact]
    public void ThenArrayHasTwoElements()
    {
        Then().Result.Has().Count(2);
        Specification.Is(
@"Given two MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has count 2");
    }

    [Fact]
    public void ThenFirstElementHaveSetup()
    {
        Then().Result.First().Name.Is(The<string>());
        Specification.Is(
@"Given two MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.First().Name is the string");
    }

    [Fact]
    public void ThenSecondElementHaveSetup()
    {
        Then().Result.Last().Name.Is(The<string>());
        Specification.Is(
@"Given two MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Last().Name is the string");
    }
}

public class GivenTwoElementsWithTransform : WhenList
{
    public GivenTwoElementsWithTransform()
        => Given().Two<MyModel>(_ => _ with { Name = A<string>() });

    [Fact]
    public void ThenSecondElementIsTransformed()
    {
        Then().Result.Last().Name.Is(The<string>());
        Specification.Is(
@"Given two MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Last().Name is the string");
    }
}

public class GivenTwoElementsWithIndexedSetup : WhenList
{
    public GivenTwoElementsWithIndexedSetup()
        => Given().Two<MyModel>((_, i) => _.Name = $"X{i + 1}");

    [Fact]
    public void ThenArrayHasTwoElements()
    {
        Then().Result.Has().Count(2);
        Specification.Is(
            """
                Given two MyModel has Name = "X{i + 1}"
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result has count 2
                """);
    }

    [Fact]
    public void ThenFirstElementHaveSetup()
    {
        Then().Result.First().Name.Is("X1");
        Specification.Is(
            """
                Given two MyModel has Name = "X{i + 1}"
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.First().Name is "X1"
                """);
    }

    [Fact]
    public void ThenSecondElementHaveSetup()
    {
        Then().Result.Last().Name.Is("X2");
        Specification.Is(
            """
                Given two MyModel has Name = "X{i + 1}"
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.Last().Name is "X2"
                """);
    }
}