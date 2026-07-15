using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenStaticModel : Spec<MyModel>
{
    [Theory]
    [InlineData("abc")]
    public void GivenDefaultSetup_ThenUseDefaultSetupOnSUT(string value)
    {
        Given<MyModel>(_ => _.Name = value)
            .When(_ => _)
            .Then().Result.Name.Is(value);
        Specification.Is(
            """
            Given MyModel has Name = value
            When _
            Then Result.Name is value
            """);
    }

    [Theory]
    [InlineData("abc")]
    public void GivenDefaultSetup_ThenUseDefaultSetupOnAValue(string value)
    {
        Given<MyModel>(_ => _.Name = value)
            .When(_ => A<MyModel>())
            .Then().Result.Name.Is(value);
        Specification.Is(
            """
            Given MyModel has Name = value
            When a MyModel
            Then Result.Name is value
            """);
    }

    [Fact]
    public void WhenNoSubjectReturnsValue()
    {
        Using(0).When(() => A<MyModel>()).Then().Result.Is(The<MyModel>());
        Specification.Is(
            """
            Using 0
            When () => A<MyModel>()
            Then Result is the MyModel
            """);
    }

    [Fact]
    public void WhenNoSubjectReturnsValueAsync()
    {
        Using(0).When(() => Task.FromResult(A<MyModel>())).Then().Result.Is(The<MyModel>());
        Specification.Is(
            """
            Using 0
            When () => Task.FromResult(A<MyModel>())
            Then Result is the MyModel
            """);
    }

    [Fact]
    public void WhenNoSubjectAction()
    {
        MyModel? model = null;
        Using(0).When(() => { model = A<MyModel>(); });
        Then();
        model!.Is(The<MyModel>());
        Specification.Is(
            """
            Using 0
            When () => { model = A<MyModel>(); }
            Then model is the MyModel
            """);
    }

    [Fact]
    public void WhenNoSubjectActionAsync()
    {
        MyModel? model = null;
        Using(0).When(() => { model = A<MyModel>(); return Task.CompletedTask; });
        Then();
        model!.Is(The<MyModel>());
        Specification.Is(
            """
            Using 0
            When () => { model = A<MyModel>(); return Task.CompletedTask; }
            Then model is the MyModel
            """);
    }

    [Theory]
    [InlineData("abc", 123)]
    public void GivenDefaultSetupAndModel_ThenUseDefaultSetupOnModel(string name, int id)
    {
        Given<MyModel>(_ => _.Name = name).Using(new MyModel { Id = id })
            .When(_ => _)
            .Then().Result.Name.Is(name).And(Result).Id.Is(id);
        Specification.Is(
            """
            Given MyModel has Name = name
            Using new MyModel { Id = id }
            When _
            Then Result.Name is name
              and Result.Id is id
            """);
    }

    [Theory]
    [InlineData("abc", 123)]
    public void GivenTwoDefaultSetup_ThenApplyBoth(string name, int id)
    {
        Given<MyModel>(_ => _.Name = name).And<MyModel>(_ => _.Id = id)
            .When(_ => _)
            .Then().Result.Name.Is(name).And(Result).Id.Is(id);
        Specification.Is(
            """
            Given MyModel has Name = name
              and MyModel has Id = id
            When _
            Then Result.Name is name
              and Result.Id is id
            """);
    }

    [Theory]
    [InlineData("abc", 123)]
    public void GivenDefaultSetupAndAModel_ThenNotUseDefaultSetupOnTheModel(string name, int id)
    {
        Given<MyModel>(_ => _.Name = name).and.A(new MyModel { Id = id })
            .When(_ => The<MyModel>())
            .Then().Result.Name.Is().Null().And(Result).Id.Is(id);
        Specification.Is(
            """
            Given MyModel has Name = name
              and a MyModel is new MyModel { Id = id }
            When the MyModel
            Then Result.Name is null
              and Result.Id is id
            """);
    }

    [Theory]
    [InlineData("abc", 123)]
    public void GivenDefaultSetupAndSpecificSetup_ThenUseBothSetupsOnTheModel(string name, int id)
    {
        Given<MyModel>(_ => _.Name = name).and.A<MyModel>(_ => _.Id = id)
            .When(_ => The<MyModel>())
            .Then().Result.Name.Is(name).And(Result).Id.Is(id);
        Specification.Is(
            """
            Given MyModel has Name = name
              and a MyModel has Id = id
            When the MyModel
            Then Result.Name is name
              and Result.Id is id
            """);
    }

    [Theory]
    [InlineData("abc", "def")]
    public void GivenDefaultSetupOverriddenBySpecificSetup_ThenUseSpecificSetupOnTheModel(string defaultName, string name)
    {
        Given<MyModel>(_ => _.Name = defaultName).and.A<MyModel>(_ => _.Name = name)
            .When(_ => The<MyModel>())
            .Then().Result.Name.Is(name);
        Specification.Is(
            """
            Given MyModel has Name = defaultName
              and a MyModel has Name = name
            When the MyModel
            Then Result.Name is name
            """);
    }

    [Theory]
    [InlineData("abc")]
    public void GivenSetupSecondModel_ThenApplySetupOn_TheSecondModel(string name)
    {
        Given().ASecond<MyModel>(_ => _.Name = name)
            .When(_ => TheSecond<MyModel>())
            .Then().Result.Name.Is(name);
        Specification.Is(
            """
            Given a second MyModel has Name = name
            When the second MyModel
            Then Result.Name is name
            """);
    }

    [Theory]
    [InlineData("abc")]
    public void GivenSetupThirdModel_ThenApplySetupOn_TheThirdModel(string name)
    {
        Given().AThird<MyModel>(_ => _.Name = name)
            .When(_ => TheThird<MyModel>())
            .Then().Result.Name.Is(name);
        Specification.Is(
            """
            Given a third MyModel has Name = name
            When the third MyModel
            Then Result.Name is name
            """);
    }

    [Theory]
    [InlineData("abc")]
    public void GivenSetupFourthModel_ThenApplySetupOn_TheFourthModel(string name)
    {
        Given().AFourth<MyModel>(_ => _.Name = name)
            .When(_ => TheFourth<MyModel>())
            .Then().Result.Name.Is(name);
        Specification.Is(
            """
            Given a fourth MyModel has Name = name
            When the fourth MyModel
            Then Result.Name is name
            """);
    }

    [Theory]
    [InlineData("abc")]
    public void GivenSetupFifthModel_ThenApplySetupOn_TheFifthModel(string name)
    {
        Given().AFifth<MyModel>(_ => _.Name = name)
            .When(_ => TheFifth<MyModel>())
            .Then().Result.Name.Is(name);
        Specification.Is(
            """
            Given a fifth MyModel has Name = name
            When the fifth MyModel
            Then Result.Name is name
            """);
    }
}