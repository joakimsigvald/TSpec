using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Provides assertion methods for verifying the order of items in an enumerable within a fluent assertion chain.
/// </summary>
/// <remarks>Use this class to assert that an enumerable meets specific order-related conditions,
/// such as descending or ascending order, optionally with a orderBy criteria. Methods return a continuation to allow
/// chaining further assertions on the enumerable.</remarks>
/// <typeparam name="TItem">The type of elements contained in the enumerable being asserted.</typeparam>
public record OrderContinuation<TItem> : EnumerableConstraint<TItem, HasEnumerableContinuation<TItem>>
{
    private readonly HasEnumerable<TItem> _parent;
    private readonly Func<TItem, TItem, int> _compare;
    private readonly string? _orderByExpr;

    internal OrderContinuation(
        HasEnumerable<TItem> parent,
        Func<TItem, TItem, int> compare,
        string? orderByExpr)
    {
        _parent = parent;
        _compare = compare;
        _orderByExpr = orderByExpr;
    }

    /// <summary>
    /// Asserts that the enumerable is ordered in descending order
    /// </summary>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> Descending()
        => OrderBy((a, b) => _compare(b, a));

    /// <summary>
    /// Asserts that the enumerable is ordered in ascending order
    /// </summary>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> Ascending()
        => OrderBy(_compare);

    private ContinueWith<HasEnumerableContinuation<TItem>> OrderBy(
        Func<TItem, TItem, int> compare, [CallerMemberName] string? methodName = null)
        => Assert(_orderByExpr is null ? Ignore.Me : (object)$"by {_orderByExpr}",
            NotNullAnd(col => AssertOrder(col, compare)),
            _orderByExpr is null ? "order" : $"order by {_orderByExpr}",
            methodName: methodName).And();

    private static void AssertOrder(IEnumerable<TItem> enumerable, Func<TItem, TItem, int> compare)
    {
        var items = enumerable.ToList();
        for (int i = 0; i < items.Count - 1; i++)
            Xunit.Assert.True(compare(items[i], items[i + 1]) <= 0);
    }

    internal override HasEnumerableContinuation<TItem> Continue() => _parent.Continue();
}
