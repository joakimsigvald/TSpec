using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations.Time;

/// <summary>
/// Object that allows assertions to be made on the provided nullable DateTime
/// </summary>
public record IsNullableDateTimeOffset : IsNullableComparableStruct<DateTimeOffset, IsNullableDateTimeOffset, IsDateTimeOffset>
{
    /// <summary>
    /// Asserts that the nullable DateTimeOffset is before the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsDateTimeOffset> Before(
        DateTimeOffset expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x < 0, expectedExpr!, "occur");

    /// <summary>
    /// Asserts that the nullable DateTimeOffset is after the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public ContinueWith<IsDateTimeOffset> After(
        DateTimeOffset expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => CompareTo(expected, x => x > 0, expectedExpr!, "occur");
}