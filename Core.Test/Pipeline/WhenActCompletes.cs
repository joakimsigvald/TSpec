using TSpec.Assert;

namespace TSpec.Test.Pipeline;

/// <summary>
/// An act either completes or throws, and a test that states nothing else has to say which.
/// The claim is positive because the fact is: the act ran to the end.
/// </summary>
public class WhenActCompletes : Spec<MyStateService, int>
{
    public WhenActCompletes() => When(_ => 1);

    [Fact]
    public void ThenSpecificationStatesTheActRan()
    {
        Then().Completes();
        Specification.Is(
            """
            When 1
            Then completes
            """);
    }
}
