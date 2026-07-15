using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided nullable struct
/// </summary>
/// <typeparam name="TValue">The underlying struct type of the nullable value to assert on</typeparam>
public record IsNullableStruct<TValue> : Constraint<TValue?, IsNullableStruct<TValue>>
    where TValue : struct
{
    /// <summary>
    /// Asserts that the value is not equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsNullableStruct<TValue>> Not(
        TValue expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => Xunit.Assert.NotEqual(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the value is not equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected not to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsNullableStruct<TValue>> Not(
        TValue? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(Describe(expected), actual => Xunit.Assert.NotEqual(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the value is null
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsNullableStruct<TValue>> Null()
        => Assert(Ignore.Me, Xunit.Assert.Null).And();
}