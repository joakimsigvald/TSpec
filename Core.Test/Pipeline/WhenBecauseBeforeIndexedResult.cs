using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// The wrapper call a chain hangs off is the pipeline's, not the reader's business — it is
/// replaced by the subject it registered. An indexer along that chain is part of the reach into
/// the result, and must not stop the wrapper from being peeled off.
/// </summary>
public class WhenBecauseBeforeIndexedResult : Spec<MyStateService, int[]>
{
    public WhenBecauseBeforeIndexedResult() => When(_ => new[] { 1, 2, 3 });

    [Fact]
    public void ThenTheIndexedResultIsNamedAlone()
    {
        Because("the array counts from one").Result[0].Is(1);
        Specification.Is(
            """
            When new { 1, 2, 3 }
            Then Result[0] is 1, because the array counts from one
            """);
    }

    [Fact]
    public void GivenNoReason_ThenTheIndexedResultIsNamedAlone()
    {
        Then().Result[0].Is(1);
        Specification.Is(
            """
            When new { 1, 2, 3 }
            Then Result[0] is 1
            """);
    }
}
