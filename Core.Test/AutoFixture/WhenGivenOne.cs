using TSpec.Assert;

namespace TSpec.Test.AutoFixture;

public class GivenOneSpecificElement : WhenList
{
    private readonly MyModel _theModel = new();
    public GivenOneSpecificElement() => Given().One(_theModel);

    [Fact]
    public void ThenElementCanBeRetrieved()
    {
        Then().Result.Has().OneItem().that.Is(_theModel);
        Specification.Is(
@"Given one MyModel is _theModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has one item that is _theModel");
    }
}

public class GivenOneElement : WhenList
{
    public GivenOneElement() => Given().One<MyModel>();

    [Fact]
    public void ThenCanRetrieveThatArray()
    {
        Then().Result.Is(The<MyModel[]>());
        Specification.Is(
@"Given one MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result is the MyModel[]");
    }

    [Fact]
    public void ThenArrayHasSingleElement()
    {
        Then().Result.Has().Count(1);
        Specification.Is(
@"Given one MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result has count 1");
    }

    [Fact]
    public void ThenElementCanBeRetrievedSeparately()
    {
        Then().Result.Single().Is(The<MyModel>());
        Specification.Is(
@"Given one MyModel
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Single() is the MyModel");
    }
}

public class GivenOneElementWithSetup : WhenList
{
    public GivenOneElementWithSetup()
        => Given().One<MyModel>(_ => _.Name = A<string>());

    [Fact]
    public void ThenArrayHasSingleElementWithSetup()
    {
        Then().Result.Single().Name.Is(The<string>());
        Specification.Is(
@"Given one MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Single().Name is the string");
    }
}

public class GivenOneElementWithTransform : WhenList
{
    public GivenOneElementWithTransform()
        => Given().One<MyModel>(_ => _ with { Name = A<string>() });

    [Fact]
    public void ThenArrayHasSingleElementWithSetup()
    {
        Then().Result.Single().Name.Is(The<string>());
        Specification.Is(
@"Given one MyModel has Name = a string
  and IMyRepository.List() returns a MyModel[]
When _.List()
Then Result.Single().Name is the string");
    }
}