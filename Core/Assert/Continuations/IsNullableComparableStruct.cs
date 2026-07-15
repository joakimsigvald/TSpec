using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Base class that allows assertions to be made on the provided nullable comparable struct
/// </summary>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
/// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
/// <typeparam name="TValueContinuation">The concrete type of the continuation for the non-null value, used by assertions that imply the value is not null</typeparam>
public abstract record IsNullableComparableStruct<TActual, TContinuation, TValueContinuation>
    : Constraint<TActual?, TContinuation>
    where TActual : struct, IComparable<TActual>
    where TContinuation : IsNullableComparableStruct<TActual, TContinuation, TValueContinuation>, new()
    where TValueContinuation : IsComparable<TActual, TValueContinuation>, new()
{
    private TValueContinuation ValueContinuation => Constraint<TActual, TValueContinuation>.Create(Actual!.Value, ActualExpr);

    /// <summary>
    /// Asserts that the value is null
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> Null() => Assert(Ignore.Me, Xunit.Assert.Null).And();

    /// <summary>
    /// Asserts that the value is not equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TContinuation> Not(
        TActual? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(expected, actual => Xunit.Assert.NotEqual(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the value is not null and not equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value, which is known to be non-null</returns>
    public ContinueWith<TValueContinuation> Not(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x != 0, expectedExpr!);

    /// <summary>
    /// Asserts that the value is greater than the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected to be greater than</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value, which is known to be non-null</returns>
    public ContinueWith<TValueContinuation> GreaterThan(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x > 0, expectedExpr!);

    /// <summary>
    /// Asserts that the value is less than the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected to be less than</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value, which is known to be non-null</returns>
    public ContinueWith<TValueContinuation> LessThan(
        TActual expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x < 0, expectedExpr!);

    internal ContinueWith<TValueContinuation> Value(
        TActual expected, string expectedExpr)
    {
        Assert(() =>
            {
                try
                {
                    Xunit.Assert.True(Actual.HasValue);
                    Xunit.Assert.Equal(expected, Actual.Value);
                }
                catch
                {
                    Xunit.Assert.Fail($"Expected {ActualExpr} to be {expected} but found {Actual}");
                }
            }, string.Empty, expectedExpr);
        return new(ValueContinuation);
    }

    private protected ContinueWith<TValueContinuation> CompareTo(
        TActual expected,
        Func<int, bool> comparer,
        string expectedExpr,
        string auxVerb = "be",
        [CallerMemberName] string? methodName = null)
    {
        Assert(expected, NotNullAnd(actual => Xunit.Assert.True(comparer(actual!.Value.CompareTo(expected)))), 
            expectedExpr, auxVerb, methodName: methodName);
        return new(ValueContinuation);
    }
}