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
            Then Result is greater than 1
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
        ex.HasMessage("Expected Result to be greater than 10 or be less than 5 but found 7",
            """
            When 7
            Then Result is greater than 10
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
            Then Result is greater than 10
                or less than 10
                and less than 9
            """);
    }

    [Fact]
    public void AndContinueWithAnotherAnd_ThenTheAndIsAsserted()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 7)
        .Then().Result.Is().either
        .GreaterThan(10).or.LessThan(10).and.LessThan(5));
        ex.HasMessage("Expected Result to be less than 5 but found 7");
    }

    [Fact]
    public void AndFirstIsTrueAndContinueWithAnd_ThenTheAndIsAsserted()
    {
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(
            () => When(_ => 3)
        .Then().Result.Is().either
        .LessThan(4).or.GreaterThan(5).and.LessThan(2));
        ex.HasMessage("Expected Result to be less than 2 but found 3");
    }

    [Fact]
    public void AndFirstIsNegated_ThenTheSecondIsNot()
    {
        When(_ => 3)
        .Then().Result.Is().either
        .not.LessThan(5).or.LessThan(4);
        Specification.Is("""
            When 3
            Then Result is not less than 5
                or less than 4
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

public class WhenEitherWithoutOr
{
    private sealed class MySpec : Spec<int> { }

    private const string NoOr = "either on Result has no or after it; write or before its last alternative";

    [Fact]
    public void AndIsTrue_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        spec.When(_ => 3).Then().Result.Is().either.GreaterThan(1);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(NoOr);
    }

    [Fact]
    public void AndIsFalse_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        spec.When(_ => 3).Then().Result.Is().either.GreaterThan(10);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(NoOr);
    }

    [Fact]
    public void AndNoWhen_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        var three = 3;
        three.Is().either.GreaterThan(10);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(
            "either on three has no or after it; write or before its last alternative");
    }

    [Fact]
    public void AndUnderscoreButNoOr_ThenThrowSetupFailed()
    {
        var spec = new MySpec();
        spec.When(_ => 3).Then().Result.Is().either.GreaterThan(1)._.LessThan(1);
        Xunit.Assert.Throws<SetupFailed>(spec.Dispose).Message.Is(NoOr);
    }

    [Fact]
    public void AndAnOrFollows_ThenDisposeCompletes()
    {
        var spec = new MySpec();
        spec.When(_ => 3).Then().Result.Is().either.GreaterThan(1).or.LessThan(1);
        spec.Dispose();
    }
}