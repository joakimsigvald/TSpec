using System.Numerics;
using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.Numerical;
using CallerArgument = System.Runtime.CompilerServices.CallerArgumentExpressionAttribute;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on integer type numbers.
/// The overloads are per-type and non-generic on purpose: a generic overload constrained to
/// <see cref="IBinaryInteger{TSelf}"/> is no better than the general <c>Is&lt;TValue&gt;</c> for overload
/// resolution (constraints do not make a candidate more specific), so the call would be ambiguous.
/// </summary>
public static class AssertionExtensionsNumerical
{
    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<byte>> Is(
        this byte actual,
        byte expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<sbyte>> Is(
        this sbyte actual,
        sbyte expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<short>> Is(
        this short actual,
        short expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<ushort>> Is(
        this ushort actual,
        ushort expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<int>> Is(
        this int actual,
        int expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<uint>> Is(
        this uint actual,
        uint expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<long>> Is(
        this long actual,
        long expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that the value is same as expected
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsIntegral<ulong>> Is(
        this ulong actual,
        ulong expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<byte> Is(
        this byte actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<byte>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<sbyte> Is(
        this sbyte actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<sbyte>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<short> Is(
        this short actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<short>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<ushort> Is(
        this ushort actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<ushort>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<int> Is(
        this int actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<int>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<uint> Is(
        this uint actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<uint>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<long> Is(
        this long actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<long>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsIntegral<ulong> Is(
        this ulong actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsIntegral<ulong>.Create(actual, actualExpr!);
}
