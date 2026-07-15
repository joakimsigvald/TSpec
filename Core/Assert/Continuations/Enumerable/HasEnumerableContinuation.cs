namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows further assertions to be made on the provided enumerable
/// </summary>
public record HasEnumerableContinuation<TItem> : HasEnumerable<TItem>
{
    /// <summary>
    /// Get available assertions for the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting the enumerable, such as equality and emptiness</returns>
    public IsEnumerable<TItem> Is() => Actual.Is(actualExpr: ActualExpr);
    /// <summary>
    /// Get available assertions for the behavior of the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting the behavior of the enumerable, such as containing an element</returns>
    public DoesEnumerable<TItem> Does() => Actual.Does(actualExpr: ActualExpr);
}