using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Numerical.Fractional;

/// <summary>
/// Base class that allows assertions to be made on the provided fractional number
/// </summary>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
/// <typeparam name="TIsFractional">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
public abstract record IsFractional<TActual, TIsFractional> : IsNumerical<TActual, TIsFractional>
    where TActual : struct, IComparable<TActual>
    where TIsFractional : IsFractional<TActual, TIsFractional>, new()
{
    /// <summary>
    /// Asserts that the actual value is approximately equal to the given value, within the provided precision
    /// </summary>
    /// <param name="expected">Expected value</param>
    /// <param name="tolerance">Allowed difference +/- expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<TIsFractional> Around(
        TActual expected, TActual tolerance, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(expected, actual => AssertEqual(expected, actual, tolerance), expectedExpr!).And();

    private protected abstract void AssertEqual(TActual expected, TActual actual, TActual tolerance);
    private protected abstract void AssertNotEqual(TActual expected, TActual actual, TActual tolerance);
}