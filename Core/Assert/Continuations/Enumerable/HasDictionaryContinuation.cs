namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows further assertions to be made on the provided dictionary
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
public record HasDictionaryContinuation<TKey, TValue> : HasDictionary<TKey, TValue>
{
    /// <summary>
    /// Get available assertions for the given dictionary as an enumerable of key-value pairs
    /// </summary>
    /// <returns>A continuation for asserting the dictionary, such as equality and emptiness</returns>
    public IsEnumerable<KeyValuePair<TKey, TValue>> Is() => Actual.Is(actualExpr: ActualExpr);
    /// <summary>
    /// Get available assertions for the behavior of the given dictionary as an enumerable of key-value pairs
    /// </summary>
    /// <returns>A continuation for asserting the behavior of the dictionary, such as containing an element</returns>
    public DoesEnumerable<KeyValuePair<TKey, TValue>> Does() => Actual.Does(actualExpr: ActualExpr);
}