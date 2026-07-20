using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.Enumerable;
using TSpec.Internal.Specification;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on dictionaries.
/// Overloads bind to <see cref="IReadOnlyDictionary{TKey, TValue}"/>; variables declared as
/// <see cref="IDictionary{TKey, TValue}"/> fall back to the enumerable-of-pairs assertions.
/// </summary>
public static class AssertionExtensionsDictionary
{
    /// <summary>
    /// Get available assertions for the given dictionary
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    /// <param name="actual">The dictionary to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the dictionary</returns>
    public static HasDictionary<TKey, TValue> Has<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue>? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => new()
        {
            Actual = actual,
            ActualExpr = actualExpr!.ParseActual(SpecificationContext.PendingSubject),
        };

    /// <summary>
    /// Asserts that the dictionary contains the given key and exposes the associated value through `that`
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    /// <param name="actual">The dictionary to assert on</param>
    /// <param name="key">The key that the dictionary is expected to contain</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="keyExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the dictionary or, through `that`, on the value</returns>
    /// <example>
    /// <code>
    /// dict.Has("a").that.Is(3)
    /// </code>
    /// </example>
    public static ContinueWithThat<HasDictionaryContinuation<TKey, TValue>, TValue> Has<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue>? actual,
        TKey key,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null,
        [CallerArgumentExpression(nameof(key))] string? keyExpr = null)
        => actual.Has(actualExpr: actualExpr).ValueForKey(key, keyExpr);
}
