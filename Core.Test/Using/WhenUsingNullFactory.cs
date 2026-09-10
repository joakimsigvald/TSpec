using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Using;

public class WhenUsingNullFactory : Spec<MyService, DateTime>
{
    /// <summary>
    /// Every other value says which arrangement it is by what it reads. A bare null says nothing,
    /// so two null factories would state one word each and the reader could not tell them apart,
    /// or that there were two. The type the value is used for is what says it.
    /// </summary>
    [Fact]
    public void ThenNameTheTypeTheNullStandsFor()
    {
        Using<IMyRepository>(() => null!)
            .And<IMySettings>(() => null!)
            .When(_ => _.GetTime())
            .Then().DoesNotThrow();
        Specification.Is(
            """
            Using null IMyRepository
              and null IMySettings
            When GetTime()
            Then does not throw
            """);
    }

    /// A null the test typed itself reads the same way, so one spelling states the one fact.
    [Fact]
    public void GivenTheNullIsCast_ThenReadItTheSameWay()
    {
        Using((IMyRepository)null!, For.Subject)
            .When(_ => _.GetTime())
            .Then().DoesNotThrow();
        Specification.Is(
            """
            Using null IMyRepository for Subject
            When GetTime()
            Then does not throw
            """);
    }
}
