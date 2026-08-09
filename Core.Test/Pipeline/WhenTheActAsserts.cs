using TSpec.Assert;
using Xunit.Sdk;

namespace TSpec.Test.Pipeline;

/// <summary>
/// An act may use TSpec.Assert itself — which is how a specification asserts that an assertion
/// fails, and what any subject that validates through TSpec.Assert does in passing. What the act
/// records internally is not the claim, so it must not reach the enclosing specification: the claim
/// is what the act did, stated by the Then that follows it.
/// </summary>
/// <remarks>
/// Without a scope of its own, an act that asserts does two kinds of damage. Its recording lands in
/// the enclosing specification as a line under no Then; and where that assertion fails, building the
/// failure message observes the enclosing specification, freezing it before the real assertion is
/// recorded — so the Then goes missing entirely.
/// </remarks>
public class WhenTheActAsserts : Spec
{
    [Fact]
    public void GivenTheActsAssertionFails_ThenStateTheThrow()
    {
        int[] arr = [1];
        When(_ => arr.Has().Count(2)).Then().Throws<XunitException>();
        Specification.Is(
            """
            When arr.Has().Count(2)
            Then throws XunitException
            """);
    }

    [Fact]
    public void GivenTheActsAssertionFails_ThenExposeItThroughThat()
    {
        int[] arr = [1];
        When(_ => arr.Has().Count(2)).Then().Throws<XunitException>()
            .that.Message.Is("Expected arr to have count 2 but found 1: [1]");
    }

    [Fact]
    public void GivenTheActsAssertionPasses_ThenStateOnlyTheAct()
    {
        int[] arr = [1];
        When(_ => arr.Has().Count(1)).Then().DoesNotThrow();
        Specification.Is(
            """
            When arr.Has().Count(1)
            Then does not throw
            """);
    }
}
