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
    where TItem : IComparable<TItem>
{
    private readonly HasEnumerable<TItem> _parent;
    private readonly Func<TItem, int>? _orderBy;
    private readonly string? _orderByExpr;

    internal OrderContinuation(
        HasEnumerable<TItem> parent,
        Func<TItem, int>? orderBy,
        string? orderByExpr)
    {
        _parent = parent;
        _orderBy = orderBy;
        _orderByExpr = orderByExpr;
    }

    /// <summary>
    /// Asserts that the enumerable is ordered in descending order
    /// </summary>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> Descending()
        => _orderBy is null
        ? OrderBy(Ignore.Me, (a, b) => b.CompareTo(a))
        : OrderBy($"by {_orderByExpr}", (a, b) => _orderBy(b).CompareTo(_orderBy(a)));

    /// <summary>
    /// Asserts that the enumerable is ordered in ascending order
    /// </summary>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> Ascending()
        => _orderBy is null
        ? OrderBy(Ignore.Me, (a, b) => a.CompareTo(b))
        : OrderBy($"by {_orderByExpr}", (a, b) => _orderBy(a).CompareTo(_orderBy(b)));

    /// <summary>
    /// Asserts that the enumerable is ordered in ascending order
    /// </summary>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    private ContinueWith<HasEnumerableContinuation<TItem>> OrderBy(
        object? expected, Func<TItem, TItem, int> compare, [CallerMemberName] string? methodName = null)
        => Assert(expected, 
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