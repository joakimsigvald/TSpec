using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Time;

/// <summary>
/// Object that allows assertions to be made on the provided TimeSpan
/// </summary>
public record IsTimeSpan : IsComparable<TimeSpan, IsTimeSpan>
{
    /// <summary>
    /// Asserts that the timeSpan is within the specified precision time from the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="tolerance">The maximum allowed difference between actual and expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsTimeSpan> CloseTo(
        TimeSpan expected, TimeSpan tolerance, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(expected, actual => Xunit.Assert.True((Actual - expected).Duration() <= tolerance), expectedExpr!).And();

    /// <summary>
    /// Asserts that the timeSpan is less than zero
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsTimeSpan> Negative()
        => Assert(Ignore.Me, actual => Xunit.Assert.True(actual < TimeSpan.Zero)).And();

    /// <summary>
    /// Asserts that the timeSpan is greater than zero
    /// </summary>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsTimeSpan> Positive()
        => Assert(Ignore.Me, actual => Xunit.Assert.True(actual > TimeSpan.Zero)).And();
}