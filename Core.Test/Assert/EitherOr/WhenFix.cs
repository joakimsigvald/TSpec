using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Assert.EitherOr;

public class WhenFix : Spec<int>
{
    [Fact]
    public void AndTheAlternativeHolds()
    {
        When(_ => 3).Then().Result.Is().either.LessThan(4).or.Fix("lower it", because: "it must fit");
        Specification.Is("""
            When 3
            Then Result is less than 4
            """);
    }

    [Fact]
    public void AndTheAlternativeFails()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => When(_ => 5).Then().Result.Is().either.LessThan(4).or.Fix("lower it", because: "it must fit"));
        ex.HasMessage("""
            Expected Result to be less than 4 but found 5
            Fix: lower it, because it must fit
            """,
            """
            When 5
            Then Result is less than 4
            """);
    }

    [Fact]
    public void AndNoReasonIsGiven()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => When(_ => 5).Then().Result.Is().either.LessThan(4).or.Fix("lower it"));
        ex.HasMessage("""
            Expected Result to be less than 4 but found 5
            Fix: lower it
            """);
    }

    [Fact]
    public void AndThereAreSeveralAlternatives()
    {
        When(_ => 3).Then().Result.Is().either.LessThan(1)._.LessThan(4).or.Fix("lower it");
        Specification.Is("""
            When 3
            Then Result is less than 1
                or less than 4
            """);
    }

    [Fact]
    public void AndSeveralAlternativesFail()
    {
        var ex = Xunit.Assert.Throws<XunitException>(
            () => When(_ => 5).Then().Result.Is().either.LessThan(1)._.LessThan(4).or.Fix("lower it"));
        ex.HasMessage("""
            Expected Result to be less than 1 or be less than 4 but found 5
            Fix: lower it
            """);
    }

    [Fact]
    public void AndItFollowsAnd_ThenThrowsSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 3).Then().Result.Is().LessThan(4).and.Fix("lower it")).Message.Is(NotAfterOr);

    [Fact]
    public void AndItFollowsEither_ThenThrowsSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 3).Then().Result.Is().either.Fix("lower it")).Message.Is(NotAfterOr);

    [Fact]
    public void AndItFollowsUnderscore_ThenThrowsSetupFailed()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => 3).Then().Result.Is().either.LessThan(1)._.Fix("lower it")).Message.Is(NotAfterOr);

    private const string NotAfterOr = "Fix is the last alternative of an either: write it after or";
}

public class WhenFixAnArchitectureRule
{
    [Fact]
    public void GivenFindings_ThenTheFailureSaysWhatToDoAndWhy()
    {
        string[] filesBesideProgramCs = ["Host.Query: Startup.cs"];
        var ex = Xunit.Assert.Throws<XunitException>(
            () => filesBesideProgramCs.Is().either.Empty().or.Fix(
                "move the file into the module that owns its logic",
                because: "a host is a pure composition root"));
        ex.HasMessage("""
            Expected filesBesideProgramCs to be empty but found 1: ["Host.Query: Startup.cs"]
            Fix: move the file into the module that owns its logic, because a host is a pure composition root
            """);
    }
}
