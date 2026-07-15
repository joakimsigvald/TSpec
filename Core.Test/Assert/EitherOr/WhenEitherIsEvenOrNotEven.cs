using TSpec.Assert;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherIsEvenOrNotEven : Spec<int>
{
    [Fact]
    public void AndSecondIsTrue()
    {
        When(_ => 1).Then().Result.Is().either.Even().or.not.Even();
        Specification.Is("""
            When 1
            Then Result is either even
                or not even
            """);
    }

    [Fact]
    public void AndFirstIsTrue()
    {
        When(_ => 2).Then().Result.Is().either.Even().or.not.Even();
        Specification.Is("""
            When 2
            Then Result is either even
                or not even
            """);
    }

    [Fact]
    public void AndThirdIsTrue_ThenFail()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 1)
        .Then().Result.Is().either
        .Even().or.Even().or.not.Even());
        ex.HasMessage("Expected Result to be even but found 1",
            """
            When 1
            Then Result is either even
                or even
            """);
    }
}