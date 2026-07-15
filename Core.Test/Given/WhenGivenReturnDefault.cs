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
            When _.GetModel()
            Then Result is the MyModel
            """);
    }

    [Fact]
    public void GivenModelSetup_ThenMockReturnDefaultWithSetup()
    {
        Given<IMyRepository>().Returns(A<MyModel>)
            .And<MyModel>(_ => _.Name = A<string>())
            .When(_ => _.GetModel())
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Given MyModel has Name = a string
              and IMyRepository returns a MyModel
            When _.GetModel()
            Then Result.Name is the string
            """);
    }
}