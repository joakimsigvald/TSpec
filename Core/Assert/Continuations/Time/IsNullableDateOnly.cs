using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Time;

/// <summary>
/// Object that allows assertions to be made on the provided nullable DateOnly
/// </summary>
public record IsNullableDateOnly : IsNullableComparableStruct<DateOnly, IsNullableDateOnly, IsDateOnly>
{
    /// <summary>
    /// Asserts that the nullable DateOnly is before the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsDateOnly> Before(
        DateOnly expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x < 0, expectedExpr!, "occur");

    /// <summary>
    /// Asserts that the nullable DateOnly is after the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsDateOnly> After(
        DateOnly expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x > 0, expectedExpr!, "occur");
}