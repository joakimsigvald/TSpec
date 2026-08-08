using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupModelWithDefault : Spec<MyService, MyModel>
{
    private const string DefaultName = "NoName";

    [Fact]
    public void GivenDefaultWithAutoMock()
    {
        Using<MyModel>(_ => _.Name = DefaultName)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using MyModel with Name = DefaultName
            When GetModel()
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenDefaultNotOverridden()
    {
        Using<MyModel>(_ => _.Name = DefaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using MyModel with Name = DefaultName
            Given IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenTwoDefaultSetups_ThenApplySecond()
    {
        Using<MyModel>(_ => _.Name = "123")
            .And<MyModel>(_ => _.Name = DefaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using MyModel with Name = "123"
              and MyModel with Name = DefaultName
            Given IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenTwoDifferentDefaultSetups_ThenApplyBoth()
    {
        Using<MyModel>(_ => _.Id = 123)
            .And<MyModel>(_ => _.Name = DefaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName).And(Result).Id.Is(123);
        Specification.Is(
            """
            Using MyModel with Id = 123
              and MyModel with Name = DefaultName
            Given IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is DefaultName
              and Result.Id is 123
            """);
    }

    [Fact]
    public void GivenDefaultValueAndDefaultSetup()
    {
        Using(DefaultName)
            .Using<MyModel>(_ => _.Name = A<string>())
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using DefaultName
              and MyModel with Name = a string
            Given IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenDefaultIsOverridden()
    {
        Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => ASecond<MyModel>())
            .When(_ => _.GetModel())
            .Using<MyModel>(_ => _.Name = DefaultName)
            .Given().ASecond<MyModel>(_ => _.Name = "Altered")
            .Then().Result.Name.Is("Altered");
        Specification.Is(
            """
            Using MyModel with Name = DefaultName
            Given a second MyModel with Name = "Altered"
              and IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is "Altered"
            """);
    }

    [Fact]
    public void GivenDefaultIsNotOverridden()
    {
        When(_ => MyService.Echo(A<MyModel>()))
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(() => The<MyModel>())
            .Using<MyModel>(_ => _.Name = DefaultName)
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using MyModel with Name = DefaultName
            Given IMyRepository.GetModel() returns the MyModel
            When MyService.Echo(a MyModel)
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenDefaultIsReplaced()
    {
        Using<MyModel>(_ => _.Name = DefaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Given().ASecond(new MyModel() { Name = "My model" })
            .Then().Result.Name.Is("My model");
        Specification.Is(
            """
            Using MyModel with Name = DefaultName
            Given a second MyModel is new MyModel() { Name = "My model" }
              and IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is "My model"
            """);
    }

    [Fact]
    public void GivenProvideDefaultSetupAfterModelIsUsedInWhen_ThenUseSetup()
    {
        Using(DefaultName)
            .Given<IMyRepository>().That(_ => _.GetModel()).Returns(ASecond<MyModel>)
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(DefaultName);
        Specification.Is(
            """
            Using DefaultName
            Given IMyRepository.GetModel() returns a second MyModel
            When GetModel()
            Then Result.Name is DefaultName
            """);
    }

    [Fact]
    public void GivenModel_ReferencedAsInputTwice_AndWithDefaultSetup_ThenUseDefaultSetup()
    {
        When(_ => MyService.Echo(A<MyModel>()))
            .Given<IMyRepository>().That(_ => _.SetModel(The<MyModel>())).Returns(() => Another<MyModel>())
            .Using<MyModel>(_ => _.Id = 123)
            .Then().Result.Id.Is(123);
        Specification.Is(
            """
            Using MyModel with Id = 123
            Given IMyRepository.SetModel(the MyModel) returns another MyModel
            When MyService.Echo(a MyModel)
            Then Result.Id is 123
            """);
    }
}

public class OverrideDefaultSetupAfterWhenReturn : Spec<MyService, MyModel>
{
    private const string TheName = "TheName";

    public OverrideDefaultSetupAfterWhenReturn() 
        => Using<MyModel>(_ => _.Name = "Something").When(_ => _.GetModel());

    [Fact]
    public void GivenDefaultSetup_ThenUseOverride()
    {
        Using<MyModel>(_ => _.Name = TheName)
            .Then().Result.Name.Is(TheName);
        Specification.Is(
            """
            Using MyModel with Name = "Something"
              and MyModel with Name = TheName
            When GetModel()
            Then Result.Name is TheName
            """);
    }
}

public class OverrideDefaultValueAfterWhenReturn : Spec<MyService, MyModel>
{
    private const string TheName = "TheName";

    public OverrideDefaultValueAfterWhenReturn()
        => Using("Something").When(_ => _.GetModel());

    [Fact]
    public void GivenDefaultValue_ThenUseDefaultValue()
    {
        Using<MyModel>(_ => _.Name = TheName).Then().Result.Name.Is(TheName);
        Specification.Is(
            """
            Using "Something"
              and MyModel with Name = TheName
            When GetModel()
            Then Result.Name is TheName
            """);
    }
}

public class OverrideDefaultSetupAfterWhenArgument : Spec<MyService, MyModel>
{
    private const string TheName = "TheName";

    public OverrideDefaultSetupAfterWhenArgument()
        => Using<MyModel>(_ => _.Name = "Something")
        .When(_ => MyService.Echo(A<MyModel>()));

    [Fact]
    public void GivenDefaultSetup_ThenUseOverride()
    {
        Using<MyModel>(_ => _.Name = TheName)
            .Then().Result.Name.Is(TheName);
        Specification.Is(
            """
            Using MyModel with Name = "Something"
              and MyModel with Name = TheName
            When MyService.Echo(a MyModel)
            Then Result.Name is TheName
            """);
    }
}

public class OverrideDefaultValueAfterWhenArgument : Spec<MyService, MyModel>
{
    private const string TheName = "TheName";

    public OverrideDefaultValueAfterWhenArgument()
        => Using("Something").When(_ => MyService.Echo(A<MyModel>()));

    [Fact]
    public void GivenDefaultValue_ThenUseDefaultValue()
    {
        Using<MyModel>(_ => _.Name = TheName).Then().Result.Name.Is(TheName);
        Specification.Is(
            """
            Using "Something"
              and MyModel with Name = TheName
            When MyService.Echo(a MyModel)
            Then Result.Name is TheName
            """);
    }

    [Fact]
    public void GivenDefaultSetup_ThenUseDefaultValue()
    {
        Using(TheName).Then().Result.Name.Is(TheName);
        Specification.Is(
            """
            Using "Something"
              and TheName
            When MyService.Echo(a MyModel)
            Then Result.Name is TheName
            """);
    }
}