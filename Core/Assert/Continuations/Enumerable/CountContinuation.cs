using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Provides assertion methods for verifying the count of items in an enumerable within a fluent assertion chain.
/// </summary>
/// <remarks>Use this class to assert that an enumerable meets specific count-related conditions, such as having
/// at least, at most, or a range of items, optionally filtered by a predicate. Methods return a continuation to allow
/// chaining further assertions on the enumerable.</remarks>
/// <typeparam name="TItem">The type of elements contained in the enumerable being asserted.</typeparam>
public record CountContinuation<TItem> : EnumerableConstraint<TItem, HasEnumerableContinuation<TItem>>
{
    private readonly HasEnumerable<TItem> _parent;
    private readonly Func<TItem, bool>? _condition;
    private readonly string? _conditionExpr;

    internal CountContinuation(
        HasEnumerable<TItem> parent, 
        Func<TItem, bool>? condition,
        string? conditionExpr)
    {
        _parent = parent;
        _condition = condition;
        _conditionExpr = conditionExpr;
    }

    /// <summary>
    /// Asserts that the enumerable has the given count
    /// </summary>
    /// <param name="expected">Lowest allowed count</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> At(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        var expectedStr = ExpressExpectation(expected, expectedExpr!);
        return Assert(expectedStr, NotNullAnd(actual => Xunit.Assert.True(Count(actual) == expected)),
            expectedStr, "have", methodName: null).And();
    }

    /// <summary>
    /// Asserts that the enumerable has at least the given count
    /// </summary>
    /// <param name="expected">Lowest allowed count</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> AtLeast(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        var expectedStr = ExpressExpectation(expected, expectedExpr!);
        return Assert(expectedStr, NotNullAnd(actual => Xunit.Assert.True(Count(actual) >= expected)),
            expectedStr, "have").And();
    }

    /// <summary>
    /// Asserts that the enumerable has at most the given count
    /// </summary>
    /// <param name="expected">Highest allowed count</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> AtMost(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        var expectedStr = ExpressExpectation(expected, expectedExpr!);
        return Assert(expected, NotNullAnd(actual => Xunit.Assert.True(Count(actual) <= expected)), 
            expectedStr, "have").And();
    }

    /// <summary>
    /// Asserts that the enumerable has count between (including) from and to
    /// </summary>
    /// <param name="from">Lowest allowed count</param>
    /// <param name="to">Highest allowed count</param>
    /// <param name="fromExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="toExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the enumerable</returns>
    public ContinueWith<HasEnumerableContinuation<TItem>> InRange(
        int from,
        int to,
        [CallerArgumentExpression(nameof(from))] string? fromExpr = null,
        [CallerArgumentExpression(nameof(to))] string? toExpr = null)
    {
        if (from > to)
            throw new SetupFailed("Given range must be in ascending order");

        var expectedStr = ExpressExpectation(from, fromExpr!, to, toExpr!);
        return Assert(expectedStr, NotNullAnd(actual =>
        {
            var actualCount = Count(actual);
            Xunit.Assert.True(actualCount >= from && actualCount <= to);
        }), expectedStr, "have",
        methodName: null).And();
    }

    internal override HasEnumerableContinuation<TItem> Continue() => _parent.Continue();

    private string ExpressExpectation(int expected, string expectedExpr) 
        => ExpressExpectation($"{Express(expectedExpr, expected)} items");

    private string ExpressExpectation(int from, string fromExpr, int to, string toExpr)
        => ExpressExpectation($"between {Express(fromExpr, from)} and {Express(toExpr, to)} items");

    private string ExpressExpectation(string expectedStr) 
        => _condition is null ? expectedStr : $"{expectedStr} where {_conditionExpr}";

    private int Count(IEnumerable<TItem> items) => _condition is null ? items.Count() : items.Count(_condition);
}