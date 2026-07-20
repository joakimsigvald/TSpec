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
        => IsEnumerable<TItem>.Create(Stabilize(actual), actualExpr!);

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
        => DoesEnumerable<TItem>.Create(Stabilize(actual), actualExpr!);

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
        => HasEnumerable<TItem>.Create(Stabilize(actual), actualExpr!);

    /// <summary>
    /// Continuations to make order-related assertions on the collection, ordered by the items themselves
    /// </summary>
    /// <typeparam name="TItem">The comparable type of the elements in the enumerable</typeparam>
    /// <param name="has">The continuation to assert order on</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static OrderContinuation<TItem> Order<TItem>(this HasEnumerable<TItem> has)
        where TItem : IComparable<TItem>
        => has.CreateOrder(Comparer<TItem>.Default.Compare, null);

    /// Deferred sequences are wrapped in a lazily-caching sequence, so that each element is
    /// produced at most once and assertion, chaining and failure description all see the same
    /// elements, while short-circuiting assertions still work on infinite sequences.
    /// Already-materialized collections are passed through by reference, preserving SameAs semantics.
    private static IEnumerable<TItem>? Stabilize<TItem>(IEnumerable<TItem>? actual)
        => actual is null or ICollection<TItem> or IReadOnlyCollection<TItem>
            ? actual
            : new CachedSequence<TItem>(actual);

    private sealed class CachedSequence<TItem>(IEnumerable<TItem> source) : IEnumerable<TItem>
    {
        private readonly List<TItem> _cache = [];
        private IEnumerator<TItem>? _source = source.GetEnumerator();

        public IEnumerator<TItem> GetEnumerator()
        {
            for (var i = 0; i < _cache.Count || TryPull(); i++)
                yield return _cache[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private bool TryPull()
        {
            if (_source is null)
                return false;

            if (_source.MoveNext())
            {
                _cache.Add(_source.Current);
                return true;
            }
            _source.Dispose();
            _source = null;
            return false;
        }
    }
}