using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupModelWithDefault : Spec<MyService, MyModel>
{
    private const string _defaultName = "NoName";

    [Fact]
    public void GivenDefaultWithAutoMock()
    {
        Given<MyModel>(_ => _.Name = _defaultName)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Given MyModel has Name = _defaultName
            When _.GetModel()
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenDefaultNotOverridden()
    {
        Given<MyModel>(_ => _.Name = _defaultName)
            .And<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Given MyModel has Name = _defaultName
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenTwoDefaultSetups_ThenApplySecond()
    {
        Given<MyModel>(_ => _.Name = "123")
            .And<MyModel>(_ => _.Name = _defaultName)
            .And<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Given MyModel has Name = "123"
              and MyModel has Name = _defaultName
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenTwoDifferentDefaultSetups_ThenApplyBoth()
    {
        Given<MyModel>(_ => _.Id = 123)
            .Given<MyModel>(_ => _.Name = _defaultName)
            .And<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName).And(Result).Id.Is(123);
        Specification.Is(
            """
            Given MyModel has Id = 123
              and MyModel has Name = _defaultName
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is _defaultName
              and Result.Id is 123
            """);
    }

    [Fact]
    public void GivenDefaultValueAndDefaultSetup()
    {
        Using(_defaultName)
            .Given<MyModel>(_ => _.Name = A<string>())
            .And<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Using _defaultName
            Given MyModel has Name = a string
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenDefaultIsOverridden()
    {
        Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Given<MyModel>(_ => _.Name = _defaultName)
            .and.ASecond<MyModel>(_ => _.Name = "Altered")
            .Then().Result.Name.Is("Altered");
        Specification.Is(
            """
            Given MyModel has Name = _defaultName
              and a second MyModel has Name = "Altered"
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is "Altered"
            """);
    }

    [Fact]
    public void GivenDefaultIsNotOverridden()
    {
        When(_ => MyService.Echo(A<MyModel>()))
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => The<MyModel>())
            .Given<MyModel>(_ => _.Name = _defaultName)
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Given MyModel has Name = _defaultName
              and IMyRepository.GetModel() returns the MyModel
            When MyService.Echo(a MyModel)
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenDefaultIsReplaced()
    {
        Given<MyModel>(_ => _.Name = _defaultName)
            .And<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Given().ASecond(new MyModel() { Name = "My model" })
            .Then().Result.Name.Is("My model");
        Specification.Is(
            """
            Given MyModel has Name = _defaultName
              and a second MyModel is new MyModel() { Name = "My model" }
              and IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is "My model"
            """);
    }

    [Fact]
    public void GivenProvideDefaultSetupAfterModelIsUsedInWhen_ThenUseSetup()
    {
        Using(_defaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(_defaultName);
        Specification.Is(
            """
            Using _defaultName
            Given IMyRepository.GetModel() returns a second MyModel
            When _.GetModel()
            Then Result.Name is _defaultName
            """);
    }

    [Fact]
    public void GivenModel_ReferencedAsInputTwice_AndWithDefaultSetup_ThenUseDefaultSetup()
    {
        When(_ => MyService.Echo(A<MyModel>()))
            .Given<IMyRepository>().That(_ => _.SetModel(The<MyModel>())).Returns(() => Another<MyModel>())
            .Given<MyModel>(_ => _.Id = 123)
            .Then().Result.Id.Is(123);
        Specification.Is(
            """
            Given MyModel has Id = 123
              and IMyRepository.SetModel(the MyModel) returns another MyModel
            When MyService.Echo(a MyModel)
            Then Result.Id is 123
            """);
    }
}

public class OverrideDefaultSetupAfterWhenReturn : Spec<MyService, MyModel>
{
    private const string _theName = "TheName";

    public OverrideDefaultSetupAfterWhenReturn() 
        => Given<MyModel>(_ => _.Name = "Something").When(_ => _.GetModel());

    [Fact]
    public void GivenDefaultSetup_ThenUseOverride()
    {
        Given<MyModel>(_ => _.Name = _theName)
            .Then().Result.Name.Is(_theName);
        Specification.Is(
            """
            Given MyModel has Name = "Something"
              and MyModel has Name = _theName
            When _.GetModel()
            Then Result.Name is _theName
            """);
    }
}

public class OverrideDefaultValueAfterWhenReturn : Spec<MyService, MyModel>
{
    private const string _theName = "TheName";

    public OverrideDefaultValueAfterWhenReturn()
        => Using("Something").When(_ => _.GetModel());

    [Fact]
    public void GivenDefaultValue_ThenUseDefaultValue()
    {
        Given<MyModel>(_ => _.Name = _theName).Then().Result.Name.Is(_theName);
        Specification.Is(
            """
            Using "Something"
            Given MyModel has Name = _theName
            When _.GetModel()
            Then Result.Name is _theName
            """);
    }
}

public class OverrideDefaultSetupAfterWhenArgument : Spec<MyService, MyModel>
{
    private const string _theName = "TheName";

    public OverrideDefaultSetupAfterWhenArgument()
        => Given<MyModel>(_ => _.Name = "Something")
        .When(_ => MyService.Echo(A<MyModel>()));

    [Fact]
    public void GivenDefaultSetup_ThenUseOverride()
    {
        Given<MyModel>(_ => _.Name = _theName)
            .Then().Result.Name.Is(_theName);
        Specification.Is(
            """
            Given MyModel has Name = "Something"
              and MyModel has Name = _theName
            When MyService.Echo(a MyModel)
            Then Result.Name is _theName
            """);
    }
}

public class OverrideDefaultValueAfterWhenArgument : Spec<MyService, MyModel>
{
    private const string _theName = "TheName";

    public OverrideDefaultValueAfterWhenArgument()
        => Using("Something").When(_ => MyService.Echo(A<MyModel>()));

    [Fact]
    public void GivenDefaultValue_ThenUseDefaultValue()
    {
        Given<MyModel>(_ => _.Name = _theName).Then().Result.Name.Is(_theName);
        Specification.Is(
            """
            Using "Something"
            Given MyModel has Name = _theName
            When MyService.Echo(a MyModel)
            Then Result.Name is _theName
            """);
    }

    [Fact]
    public void GivenDefaultSetup_ThenUseDefaultValue()
    {
        Using(_theName).Then().Result.Name.Is(_theName);
        Specification.Is(
            """
            Using "Something"
              and _theName
            When MyService.Echo(a MyModel)
            Then Result.Name is _theName
            """);
    }
}