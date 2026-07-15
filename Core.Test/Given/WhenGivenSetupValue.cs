using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Given;

public class WhenGivenSetupValue : Spec<MyService, DateTime>
{
    private static readonly DateTime _now = DateTime.Now;
    private static readonly DateTime _anotherTime = DateTime.Now.AddDays(1);

    [Fact]
    public void ThenCanApplySpecificValueForPreviouslyMentionedType()
    {
        Using(A<DateTime>)
            .When(_ => _.GetTime())
            .Given().A(_now)
            .Then().Result.Is(_now);
        Specification.Is(
            """
            Using a DateTime
            Given a DateTime is _now
            When _.GetTime()
            Then Result is _now
            """);
    }

    [Fact]
    public void ThenApplyFirstSpecifiedValueForPreviouslyMentionedType()
    {
        Using(A<DateTime>)
            .When(_ => _.GetTime())
            .Given().A(_now)
            .and.A(_anotherTime) //Ignore this since a specific value has already been provided
            .Then().Result.Is(_now);
        Specification.Is(
            """
            Using a DateTime
            Given a DateTime is _anotherTime
              and a DateTime is _now
            When _.GetTime()
            Then Result is _now
            """);
    }
}