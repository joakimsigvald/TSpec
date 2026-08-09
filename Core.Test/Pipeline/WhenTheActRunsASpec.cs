using TSpec.Assert;
using TSpec.Test.TestData;

namespace TSpec.Test.Pipeline;

/// <summary>
/// A specification whose subject is another specification. The inner pipeline's setup failure is
/// then an outcome of the act, and belongs to the outer specification's claim like any other throw —
/// where the same failure raised by the outer pipeline itself is not an outcome at all, but the
/// author's own mistake, and must keep escaping.
/// </summary>
/// <remarks>
/// The distinction is which pipeline raised it: a failure that has already left a pipeline came from
/// a nested one, while the outer pipeline's own is still inside the frame that is catching.
/// </remarks>
public class WhenTheActRunsASpec : Spec<int>
{
    private sealed class InnerSpec : Spec<MyStateService, int> { }

    [Fact]
    public void GivenTheInnerSetupFails_ThenTheOuterObservesTheThrow()
        => When(_ => new InnerSpec()
                .Given().A<MyModel>(m => throw new ApplicationException())
                .When(s => s.Counter)
                .Then().Result)
            .Then().Throws<SetupFailed>();

    [Fact]
    public void GivenTheInnerSetupFails_ThenTheOuterReachesTheCause()
        => When(_ => new InnerSpec()
                .Given().A<MyModel>(m => throw new ApplicationException())
                .When(s => s.Counter)
                .Then().Result)
            .Then().Throws<SetupFailed>().that.InnerException.Is().A<ApplicationException>();

    /// <summary>
    /// The boundary: a failure raised while the inner specification is still being *configured* has
    /// not run a pipeline, so nothing marks it as coming from one, and the outer specification
    /// cannot tell it from its own. Declaring the act twice is the clearest case — it throws from
    /// the fluent call itself, before anything executes.
    /// </summary>
    /// <remarks>
    /// Recorded as a limitation rather than a defect: the rule is that a failure which has left a
    /// pipeline came from a nested one, and this failure never entered a pipeline to leave it.
    /// Closing it means marking at the throw sites, which is a wider change than this rule.
    /// </remarks>
    [Fact]
    public void GivenTheInnerFailsWhileBeingConfigured_ThenItStillEscapes()
        => Xunit.Assert.Throws<SetupFailed>(
            () => When(_ => new InnerSpec().When(s => s.Counter).When(s => s.Counter * 2).Then().Result)
                .Then().Throws<SetupFailed>());
}

/// <summary>
/// The guard: a pipeline's own setup failure is not an outcome it may report. Swallowing it would
/// let a misconfigured specification pass as one that expected to fail.
/// </summary>
public class WhenTheSpecItselfIsMisconfigured : Spec<MyStateService, int>
{
    [Fact]
    public void ThenTheFailureEscapesRatherThanBecomingAnOutcome()
        => Xunit.Assert.Throws<SetupFailed>(
            () => Given().A<MyModel>(m => throw new ApplicationException())
                .When(_ => _.Counter)
                .Then().Throws<SetupFailed>());
}
