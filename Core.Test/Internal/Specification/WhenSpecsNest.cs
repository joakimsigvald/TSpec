using TSpec.Assert;

namespace TSpec.Test.Internal.Specification;

/// <summary>
/// The ambient context that assertions record into is one slot per async flow, so a spec
/// constructed inside another one shares it. Constructing the inner spec must not take the slot
/// from the enclosing spec for good, and disposing it must hand the slot back rather than blank it —
/// otherwise the enclosing spec keeps running with no context of its own, and its assertions are
/// recorded where nobody reads them. The test still passes; only the specification is wrong, which
/// is the failure mode that hides.
/// </summary>
public class WhenSpecsNest : Spec<int>
{
    private sealed class InnerSpec : Spec<int> { }

    [Fact]
    public void GivenNoNesting_ThenRecordTheAssertion()
    {
        When(_ => 1).Then().Result.Is(1);
        Specification.Is(
            """
            When 1
            Then Result is 1
            """);
    }

    [Fact]
    public void GivenANestedSpecWasConstructed_ThenStillRecordTheEnclosingAssertion()
    {
        using var inner = new InnerSpec();
        When(_ => 1).Then().Result.Is(1);
        Specification.Is(
            """
            When 1
            Then Result is 1
            """);
    }

    [Fact]
    public void GivenANestedSpecWasDisposed_ThenStillRecordTheEnclosingAssertion()
    {
        new InnerSpec().Dispose();
        When(_ => 1).Then().Result.Is(1);
        Specification.Is(
            """
            When 1
            Then Result is 1
            """);
    }

    /// <summary>
    /// One specification per test: asserting on a specification is itself a recorded step, so a test
    /// that pinned both would find the first assertion written into the second's record.
    /// </summary>
    [Fact]
    public void GivenANestedSpecRanItsOwnPipeline_ThenTheNestedOneKeepsItsOwn()
    {
        using var inner = new InnerSpec();
        inner.When(_ => 2).Then().Result.Is(2);
        When(_ => 1).Then().Result.Is(1);
        inner.Specification.Is(
            """
            When 2
            Then Result is 2
            """);
    }

    [Fact]
    public void GivenANestedSpecRanItsOwnPipeline_ThenTheEnclosingOneKeepsItsOwn()
    {
        using var inner = new InnerSpec();
        inner.When(_ => 2).Then().Result.Is(2);
        When(_ => 1).Then().Result.Is(1);
        Specification.Is(
            """
            When 1
            Then Result is 1
            """);
    }
}
