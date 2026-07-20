using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.String;
using TSpec.Internal.Specification;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on string
/// </summary>
public static class AssertionExtensionsString
{
    /// <summary>
    /// Verify that the string is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public static ContinueWith<IsStringContinuation> Is(
        this string? actual,
        string? expected,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected!, expectedExpr!);

    /// <summary>
    /// Get available assertions for the given string
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsString Is(
        this string? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => IsString.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the characteristics of the given string
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static DoesString Does(
        this string? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => DoesString.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the characteristics of the given string, such as its length
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static HasString Has(
        this string? actual,
        Ignore _ = default,
        [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => new()
        {
            Actual = actual,
            ActualExpr = actualExpr!.ParseActual(SpecificationContext.PendingSubject),
        };
}