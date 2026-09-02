using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// The wrapper call a chain hangs off is the pipeline's, not the reader's business — it is
/// replaced by the subject it registered. Type arguments spelled out along that chain are part of
/// the reach into the result, and must not stop the wrapper from being peeled off.
/// </summary>
public class WhenBecauseBeforeGenericCall : Spec<MyStateService, object[]>
{
    public WhenBecauseBeforeGenericCall() => When(_ => [1, "two", 3]);

    [Fact]
    public void ThenTheGenericCallIsNamedAlone()
    {
        Because("two of the three are ints").Result.OfType<int>().Count().Is(2);
        Specification.Is(
            """
            When [1, "two", 3]
            Then Result.OfType<int>().Count() is 2, because two of the three are ints
            """);
    }

    [Fact]
    public void GivenNoReason_ThenTheGenericCallIsNamedAlone()
    {
        Then().Result.OfType<int>().Count().Is(2);
        Specification.Is(
            """
            When [1, "two", 3]
            Then Result.OfType<int>().Count() is 2
            """);
    }
}
