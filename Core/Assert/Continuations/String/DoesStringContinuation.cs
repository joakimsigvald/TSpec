namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Object that allows further assertions to be made on the provided string
/// </summary>
public record DoesStringContinuation : DoesString
{
    /// <summary>
    /// Continuation to assert that the string is satisfying some expectation
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public IsString Is() => Actual.Is(actualExpr: ActualExpr);
}