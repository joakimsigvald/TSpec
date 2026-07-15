using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenAnyValue : Spec<MyService, MyModel>
{
    [Fact]
    public void GivenAnyModelWithSetup_ThenUseSetup()
    {
        When(_ => MyService.Echo(Any<MyModel>(_ => _.Name = A<string>())))
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            When MyService.Echo(any MyModel { Name = a string })
            Then Result.Name is the string
            """);
    }
}