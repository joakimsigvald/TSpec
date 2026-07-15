namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows further assertions to be made on the provided enumerable
/// </summary>
public record DoesEnumerableContinuation<TItem> : DoesEnumerable<TItem>
{
    /// <summary>
    /// Get available assertions for the characteristics of the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting characteristics of the enumerable, such as count and order</returns>
    public HasEnumerable<TItem> Has() => Actual.Has(actualExpr: ActualExpr);
    /// <summary>
    /// Get available assertions for the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting the enumerable, such as equality and emptiness</returns>
    public IsEnumerable<TItem> Is() => Actual.Is(actualExpr: ActualExpr);
}