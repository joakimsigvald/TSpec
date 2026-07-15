using TSpec.Assert;

namespace TSpec.Test.Assert;

public class WhenDateTime : Spec<DateTime>
{
    [Fact]
    public void IsSame()
    {
        When(_ => A(_)).Then().Result.Is(The<DateTime>());
        Specification.Is(
            """
            When a _
            Then Result is the DateTime
            """);
    }

    [Fact]
    public void IsNot()
    {
        When(_ => A(_)).Then().Result.Is().Not(Another<DateTime>());
        Specification.Is(
            """
            When a _
            Then Result is not another DateTime
            """);
    }

    [Fact]
    public void IsBeforeEtc()
    {
        When(_ => A(_))
            .Then().Result.Is().Before(The<DateTime>().AddDays(1))
            .and.After(The<DateTime>().AddDays(-1))
            .and.not.Before(The<DateTime>())
            .and.not.After(The<DateTime>());
        Specification.Is(
            """
            When a _
            Then Result is before the DateTime's AddDays(1)
                and after the DateTime's AddDays(-1)
                and not before the DateTime
                and not after the DateTime
            """);
    }

    [Fact]
    public void BeforeFailWithMessage() 
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => A(_)).Then().Result.Is().Before(The<DateTime>().AddDays(-1)));
        ex.Message.Is($"Expected Result to occur before {The<DateTime>().AddDays(-1)} but found {The<DateTime>()}");
    }
}