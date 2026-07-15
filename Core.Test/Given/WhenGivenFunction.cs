using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenFunction : Spec<MyService, DateTime>
{
    [Fact]
    public void UsingFunction_CanBeUsedAsDefaultFunction()
    {
        Using(A<DateTime>).When(_ => _.GetTime())
            .Then().Result.Is(The<DateTime>());
        Specification.Is(
            """
            Using a DateTime
            When _.GetTime()
            Then Result is the DateTime
            """);
    }

    [Fact]
    public void UsingValue_CanBeUsedForDefaultFunction()
    {
        Using(A<DateTime>()).When(_ => _.GetTime())
            .Then().Result.Is(The<DateTime>());
        Specification.Is(
            """
            Using a DateTime
            When _.GetTime()
            Then Result is the DateTime
            """);
    }
}