using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class GivenThreeElements : WhenList
{
    public GivenThreeElements() => Given().Three<MyModel>();

    [Fact]
    public void ThenArrayHasThreeElements()
    {
        Then().Result.Has().Count(3);
        Specification.Is(
            """
                Given three MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result has count 3
                """);
    }

    [Fact]
    public void ThenTheElementsAreDifferent()
    {
        Then().Result.First().Is().Not(Result.Last());
        Specification.Is(
            """
                Given three MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.First() is not Result.Last()
                """);
    }

    [Fact]
    public void ThenThirdElementCanBeRetrievedSeparately()
    {
        Then().Result.Last().Is(TheThird<MyModel>());
        Specification.Is(
            """
                Given three MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.Last() is the third MyModel
                """);
    }
}

public class GivenFourElements : WhenList
{
    public GivenFourElements() => Given().Four<MyModel>();

    [Fact]
    public void ThenArrayHasFourElements()
    {
        Then().Result.Has().Count(4);
        Specification.Is(
            """
                Given four MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result has count 4
                """);
    }

    [Fact]
    public void ThenFourthElementCanBeRetrievedSeparately()
    {
        Then().Result.Last().Is(TheFourth<MyModel>());
        Specification.Is(
            """
                Given four MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.Last() is the fourth MyModel
                """);
    }
}

public class GivenFiveElements : WhenList
{
    public GivenFiveElements() => Given().Five<MyModel>();

    [Fact]
    public void ThenArrayHasFiveElements()
    {
        Then().Result.Has().Count(5);
        Specification.Is(
            """
                Given five MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result has count 5
                """);
    }

    [Fact]
    public void ThenFifthElementCanBeRetrievedSeparately()
    {
        Then().Result.Last().Is(TheFifth<MyModel>());
        Specification.Is(
            """
                Given five MyModel
                  and IMyRepository.List() returns a MyModel[]
                When _.List()
                Then Result.Last() is the fifth MyModel
                """);
    }
}

public class GivenSomeSpecificElements : WhenList
{
    private readonly MyModel _one = new() { Id = 1 };
    private readonly MyModel _two = new() { Id = 2 };
    private readonly MyModel _three = new() { Id = 3 };

    public GivenSomeSpecificElements() => Given().Some([_one, _two, _three]);

    [Fact]
    public void ThenElementsCanBeRetrieved()
    {
        Result.Has().Count(3);
        Result[0].Id.Is(1);
        Result[1].Id.Is(2);
        Result[2].Id.Is(3);
        Specification.Is(
@"Given some MyModel is [_one, _two, _three]
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has count 3
Result[0].Id is 1
Result[1].Id is 2
Result[2].Id is 3");
    }

    [Fact]
    public void ThenElementsCanBeReferred() 
    {
        Then();
        Some<MyModel>()[2].Id.Is(3);
        Many<MyModel>()[2].Id.Is(3);
        The<MyModel[]>()[2].Id.Is(3);
        TheThird<MyModel>().Id.Is(3);
    }
}