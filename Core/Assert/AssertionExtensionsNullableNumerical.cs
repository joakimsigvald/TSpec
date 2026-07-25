using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.Numerical.Nullable;
using CallerArgument = System.Runtime.CompilerServices.CallerArgumentExpressionAttribute;

namespace TSpec.Assert;

/// <summary>
/// Fluent assertions on nullable integer type numbers.
/// Per-type and non-generic for the same overload-resolution reason as
/// <see cref="AssertionExtensionsNumerical"/>.
/// </summary>
public static class AssertionExtensionsNullableNumerical
{
    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<byte>> Is(
        this byte? actual,
        byte? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<sbyte>> Is(
        this sbyte? actual,
        sbyte? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<short>> Is(
        this short? actual,
        short? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<ushort>> Is(
        this ushort? actual,
        ushort? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<int>> Is(
        this int? actual,
        int? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<uint>> Is(
        this uint? actual,
        uint? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<long>> Is(
        this long? actual,
        long? expected,
        [CallerArgument(nameof(actual))] string? actualExpr = null,
        [CallerArgument(nameof(expected))] string? expectedExpr = null)
        => actual.Is(actualExpr: actualExpr!).Value(expected, expectedExpr!);

    /// <summary>
    /// Verify that actual is expected and return continuation for further assertions of the value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="expected">The expected value</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static ContinueWith<IsNullableIntegral<ulong>> Is(
        this ulong? actual,
        ulong? expected,
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
    public static IsNullableIntegral<byte> Is(
        this byte? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<byte>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<sbyte> Is(
        this sbyte? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<sbyte>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<short> Is(
        this short? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<short>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<ushort> Is(
        this ushort? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<ushort>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<int> Is(
        this int? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<int>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<uint> Is(
        this uint? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<uint>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<long> Is(
        this long? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<long>.Create(actual, actualExpr!);

    /// <summary>
    /// Get available assertions for the given value
    /// </summary>
    /// <param name="actual">The value to assert on</param>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="actualExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the value</returns>
    public static IsNullableIntegral<ulong> Is(
        this ulong? actual,
        Ignore _ = default,
        [CallerArgument(nameof(actual))] string? actualExpr = null)
        => IsNullableIntegral<ulong>.Create(actual, actualExpr!);
}
