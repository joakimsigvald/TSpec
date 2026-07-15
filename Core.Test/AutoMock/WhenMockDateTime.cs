using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockDateTime : Spec<StaticDateService, DateTime>
{
    public WhenMockDateTime() => When(_ => _.GetDate());
    public class GivenItWasNotProvided : WhenMockDateTime
    {
        [Fact]
        public void Then_It_Has_RandomDateTime()
        {
            Then().Result.Is().Not(A<DateTime>()).And(Result).Ticks.Is().Not(0);
            Specification.Is(
                """
                When _.GetDate()
                Then Result is not a DateTime
                  and Result.Ticks is not 0
                """);
        }
    }

    public class GivenItWasProvided : WhenMockDateTime
    {
        [Fact]
        public void Then_It_Has_ProvidedValue()
        {
            Using(A<DateTime>()).Then().Result.Is(The<DateTime>());
            Specification.Is(
                """
                Using a DateTime
                When _.GetDate()
                Then Result is the DateTime
                """);
        }
    }
}

public class StaticDateService(DateTime date)
{
    private readonly DateTime _date = date;
    public DateTime GetDate() => _date;
}