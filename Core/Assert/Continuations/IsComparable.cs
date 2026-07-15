using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided comparable
/// </summary>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
/// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
public abstract record IsComparable<TActual, TContinuation> : Constraint<TActual, TContinuation>
    where TContinuation : IsComparable<TActual, TContinuation>, new()
    where TActual : IComparable<TActual>
{
    /// <summary>
    /// Asserts that the value is not equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> Not(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(expected, actual => Xunit.Assert.NotEqual(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the value is greater than expected
    /// </summary>
    /// <param name="expected">The value that actual is expected to be greater than</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> GreaterThan(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x > 0, expectedExpr!);

    /// <summary>
    /// Asserts that the value is less than expected
    /// </summary>
    /// <param name="expected">The value that actual is expected to be less than</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> LessThan(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x < 0, expectedExpr!);

    private protected ContinueWith<TContinuation> CompareTo(
        TActual expected,
        Func<int, bool> comparer,
        string expectedExpr,
        string auxVerb = "be",
        [CallerMemberName] string? methodName = null)
        => Assert(expected, 
            NotNullAnd(actual => Xunit.Assert.True(comparer(actual.CompareTo(expected)))), 
            expectedExpr, auxVerb, methodName: methodName)
        .And();
}

/// <summary>
/// Object that allows assertions to be made on the provided comparable
/// </summary>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
public record IsComparable<TActual> : IsComparable<TActual, IsComparable<TActual>>
    where TActual : IComparable<TActual>
{
}