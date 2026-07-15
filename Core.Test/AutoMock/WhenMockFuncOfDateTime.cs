using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class WhenMockFuncOfDateTime : Spec<DateService, DateTime>
{
    public WhenMockFuncOfDateTime() => When(_ => _.GetNow());

    public class GivenItWasNotProvided : WhenMockFuncOfDateTime
    {
        [Fact]
        public void Then_It_Has_RandomDateTime()
        {
            Then().Result.Is().Not(The<DateTime>()).And(Result).Ticks.Is().Not(0);
            Specification.Is(
                """
                When _.GetNow()
                Then Result is not the DateTime
                  and Result.Ticks is not 0
                """);
        }
    }

    public class GivenItWasProvided : WhenMockFuncOfDateTime
    {
        [Fact]
        public void Then_It_Has_ProvidedValue()
        {
            Using(A<DateTime>()).Then().Result.Is(The<DateTime>());
            Specification.Is(
                """
                Using a DateTime
                When _.GetNow()
                Then Result is the DateTime
                """);
        }
    }
}

public class DateService(Func<DateTime> now)
{
    private readonly Func<DateTime> _now = now;
    public DateTime GetNow() => _now();
}