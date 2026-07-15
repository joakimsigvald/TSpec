namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows further assertions to be made on the provided enumerable
/// </summary>
public record IsEnumerableContinuation<TItem> : IsEnumerable<TItem>
{
    /// <summary>
    /// Get available assertions for the characteristics of the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting characteristics of the enumerable, such as count and order</returns>
    public HasEnumerable<TItem> Has() => Actual.Has(actualExpr: ActualExpr);
    /// <summary>
    /// Get available assertions for the behavior of the given enumerable
    /// </summary>
    /// <returns>A continuation for asserting the behavior of the enumerable, such as containing an element</returns>
    public DoesEnumerable<TItem> Does() => Actual.Does(actualExpr: ActualExpr);
}