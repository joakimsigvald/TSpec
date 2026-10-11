using TSpec.Assert;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherHasMoreAlternatives : Spec<int>
{
    [Fact]
    public void AndTheLastIsTrue()
    {
        When(_ => 3).Then().Result.Is().either.LessThan(1)._.GreaterThan(5).or.LessThan(4);
        Specification.Is("""
            When 3
            Then Result is less than 1, greater than 5
                or less than 4
            """);
    }

    [Fact]
    public void AndTheMiddleIsTrue()
    {
        When(_ => 7).Then().Result.Is().either.LessThan(1)._.GreaterThan(5).or.LessThan(4);
        Specification.Is("""
            When 7
            Then Result is less than 1, greater than 5
                or less than 4
            """);
    }

    [Fact]
    public void AndNoneIsTrue()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 4).Then().Result.Is().either.LessThan(1)._.GreaterThan(5).or.LessThan(4));
        ex.HasMessage("Expected Result to be less than 1, be greater than 5 or be less than 4 but found 4",
            """
            When 4
            Then Result is less than 1, greater than 5
                or less than 4
            """);
    }

    [Fact]
    public void AndWrittenWithOrEither()
    {
        When(_ => 3).Then().Result.Is().either.LessThan(1).or.either.GreaterThan(5).or.LessThan(4);
        Specification.Is("""
            When 3
            Then Result is less than 1, greater than 5
                or less than 4
            """);
    }

    [Fact]
    public void AndNoneIsTrueWrittenWithOrEither()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 4).Then().Result.Is().either.LessThan(1).or.either.GreaterThan(5).or.LessThan(4));
        ex.HasMessage("Expected Result to be less than 1, be greater than 5 or be less than 4 but found 4");
    }

    [Fact]
    public void AndUnderscoreWithoutEither_ThenThrowsSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 4).Then().Result.Is().GreaterThan(1)._.LessThan(5));
}
