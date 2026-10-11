using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Object that allows assertions to be made on the provided bool
/// </summary>
public record IsBool : Constraint<bool, IsBool>
{
    /// <summary>
    /// Asserts that the value is equal to the given value
    /// </summary>
    /// <param name="expected">The value that actual is expected to be</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsBool> EqualTo(
        bool expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(expected, actual => Xunit.Assert.Equal(expected, actual), expectedExpr!).And();

    /// <summary>
    /// Asserts that the value is true
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsBool> True() => Assert(Ignore.Me, Xunit.Assert.True).And();

    /// <summary>
    /// Asserts that the value is false
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsBool> False() => Assert(Ignore.Me, Xunit.Assert.False).And();
}