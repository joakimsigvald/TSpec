using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.Enumerable;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on enumerables
/// </summary>
public static class AssertionExtensionsEnumerable
{
    /// <summary>
    /// Verify that both enumerables are the same instance
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the enumerable</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsEnumerableContinuation<TItem>> Is<TItem>(
        this IEnumerable<TItem>? actual,
        IEnumerable<TItem> expected,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).SameAs(expected, expectedExpr!);

    /// <summary>
    /// Get available assertions for the given enumerable
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the enumerable</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsEnumerable<TItem> Is<TItem>(
        this IEnumerable<TItem>? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => IsEnumerable<TItem>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given enumerable
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the enumerable</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static DoesEnumerable<TItem> Does<TItem>(
        this IEnumerable<TItem>? actual,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => DoesEnumerable<TItem>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for enumerable
    /// </summary>
    /// <typeparam name="TItem">The type of the elements in the enumerable</typeparam>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static HasEnumerable<TItem> Has<TItem>(
        this IEnumerable<TItem>? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => HasEnumerable<TItem>.Create(actual, actualExpr!);
}