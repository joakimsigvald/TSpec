using TSpec.Assert;

namespace TSpec.Test.Assert.EitherOr;

public class WhenEitherIsGreaterThanOrLessThan : Spec<int>
{
    [Fact]
    public void AndBothIsTrue()
    {
        When(_ => 3)
        .Then().Result.Is().either
        .GreaterThan(1).or.LessThan(5);
        Specification.Is("""
            When 3
            Then Result is either greater than 1
                or less than 5
            """);
    }

    [Fact]
    public void AndNoneIsTrue()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 7)
        .Then().Result.Is().either
        .GreaterThan(10).or.LessThan(5));
        ex.HasMessage("Expected Result to be greater than 10 but found 7",
            """
            When 7
            Then Result is either greater than 10
                or less than 5
            """);
    }

    [Fact]
    public void AndContinueWithAnd_ThenThrowsSetupFailed()
    {
        Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 7)
        .Then().Result.Is().either
        .GreaterThan(10).and.LessThan(5));
    }

    [Fact]
    public void AndContinueWithAnotherOr_ThenThrowsSetupFailed()
    {
        Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 7)
        .Then().Result.Is().either
        .GreaterThan(10).or.LessThan(10).or.LessThan(10));
    }

    [Fact]
    public void AndContinueWithAnotherAnd_ThenExecuteAndIfOrSucceeded()
    {
        When(_ => 7)
        .Then().Result.Is().either
        .GreaterThan(10).or.LessThan(10).and.LessThan(9);
        Specification.Is("""
            When 7
            Then Result is either greater than 10
                or less than 10
                and less than 9
            """);
    }

    [Fact]
    public void AndPrecededWithNot_ThenThrowsSetupFailed()
    {
        Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 7)
        .Then().Result.Is().not.either
        .GreaterThan(10).or.LessThan(5));
    }
}

public class WhenEitherWithoutOr : Spec<int>
{
    [Fact]
    public void AndIsTrue()
    {
        When(_ => 3)
        .Then().Result.Is().either
        .GreaterThan(1);
        Specification.Is("""
            When 3
            Then Result is either greater than 1
            """);
    }

    [Fact]
    public void AndIsFalse_ThenDoesNotThrow()
    {
        When(_ => 3)
        .Then().Result.Is().either
        .GreaterThan(10);
        Specification.Is("""
            When 3
            Then Result is either greater than 10
            """);
    }
}