using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenReturnDefault : Spec<MyService, MyModel>
{
    [Fact]
    public void ThenMockReturnDefault()
    {
        Given<IMyRepository>().Returns(A<MyModel>)
            .When(_ => _.GetModel())
            .Then().Result.Is(The<MyModel>());
        Specification.Is(
            """
            Given IMyRepository returns a MyModel
            When GetModel()
            Then Result is the MyModel
            """);
    }

    [Fact]
    public void GivenModelSetup_ThenMockReturnDefaultWithSetup()
    {
        Given<IMyRepository>().Returns(A<MyModel>)
            .Using<MyModel>(_ => _.Name = A<string>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Using MyModel with Name = a string
            Given IMyRepository returns a MyModel
            When GetModel()
            Then Result.Name is the string
            """);
    }
}