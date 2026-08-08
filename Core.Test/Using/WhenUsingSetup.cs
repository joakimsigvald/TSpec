using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingSetup : Spec<MyService, MyModel>
{
    [Fact]
    public void WithAction_ThenReadsAsATypeArrangement()
    {
        Using<MyModel>(_ => _.Name = A<string>())
            .When(_ => MyService.Echo(The<MyModel>()))
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Using MyModel with Name = a string
            When MyService.Echo(the MyModel)
            Then Result.Name is the string
            """);
    }

    [Fact]
    public void AfterAnotherUsing_ThenJoinsTheUsingRun()
    {
        Using("DefaultName")
            .And<MyModel>(_ => _.Name = A<string>())
            .When(_ => MyService.Echo(The<MyModel>()))
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Using "DefaultName"
              and MyModel with Name = a string
            When MyService.Echo(the MyModel)
            Then Result.Name is the string
            """);
    }
}

public class WhenUsingTransform : Spec<MyService, MyRecord>
{
    [Fact]
    public void ThenReadsAsATypeArrangement()
    {
        Using<MyRecord>(_ => _ with { Name = A<string>(), Id = 1 })
            .When(_ => MyService.Echo(The<MyRecord>()))
            .Then().Result.Name.Is(The<string>());
        Specification.Is(
            """
            Using MyRecord with Name = a string, Id = 1
            When MyService.Echo(the MyRecord)
            Then Result.Name is the string
            """);
    }
}
