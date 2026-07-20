namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Object that allows further assertions to be made on the provided string
/// </summary>
public record HasStringContinuation : HasString
{
    /// <summary>
    /// Get available assertions for the given string
    /// </summary>
    /// <returns>A continuation for asserting the string, such as equality and emptiness</returns>
    public IsString Is() => (Actual as string).Is(actualExpr: ActualExpr);
    /// <summary>
    /// Get available assertions for the characteristics of the given string
    /// </summary>
    /// <returns>A continuation for asserting the behavior of the string, such as containing a substring</returns>
    public DoesString Does() => (Actual as string).Does(actualExpr: ActualExpr);
}
