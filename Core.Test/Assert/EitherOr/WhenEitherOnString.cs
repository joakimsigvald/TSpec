using TSpec.Assert;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherOnString : Spec<string>
{
    [Fact]
    public void AndSecondIsTrue()
    {
        When(_ => "").Then().Result.Is().either.Null().or.Empty();
        Specification.Is("""
            When ""
            Then Result is null
                or empty
            """);
    }

    [Fact]
    public void AndNoneIsTrue()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => "a").Then().Result.Is().either.Null().or.Empty());
        ex.HasMessage("Expected Result to be null or be empty but found \"a\"",
            """
            When "a"
            Then Result is null
                or empty
            """);
    }

    [Fact]
    public void AndDoes()
    {
        When(_ => "abc").Then().Result.Does().either.StartWith("x").or.EndWith("c");
        Specification.Is("""
            When "abc"
            Then Result starts with "x"
                or ends with "c"
            """);
    }
}
