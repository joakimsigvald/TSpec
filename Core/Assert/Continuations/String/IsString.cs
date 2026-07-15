using System.Runtime.CompilerServices;
using static Xunit.Assert;

namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Object that allows assertions to be made on the provided string
/// </summary>
public record IsString : StringConstraint<IsStringContinuation>
{
    /// <summary>
    /// Asserts that the string is equivalent to expected, ignoring casing and leading or trailing whitespace
    /// </summary>
    /// <remarks>Synonymous with <see cref="EquivalentTo"/></remarks>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> Like(
        string? expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => Equal(actual?.Trim().ToLower(), 
                expected?.Trim().ToLower()), 
            expectedExpr!).And();

    /// <summary>
    /// Asserts that the string is equivalent to expected, ignoring casing and leading or trailing whitespace
    /// </summary>
    /// <remarks>Synonymous with <see cref="Like"/></remarks>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> EquivalentTo(
        string? expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => Equal(actual?.Trim().ToLower(), expected?.Trim().ToLower()), 
            expectedExpr!).And();

    /// <summary>
    /// Asserts that the string is null
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> Null()
        => Assert(Ignore.Me, actual => Xunit.Assert.Null(Actual)).And();

    /// <summary>
    /// Asserts that the string is empty
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> Empty()
        => Assert(Ignore.Me, NotNullAnd(Xunit.Assert.Empty)).And();

    /// <summary>
    /// Asserts that the string is null or empty
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> NullOrEmpty()
        => Assert(Ignore.Me, actual => Xunit.Assert.Empty(actual ?? string.Empty)).And();

    /// <summary>
    /// Asserts that the string does not contain non-whitespace characters
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> NullOrWhitespace()
        => Assert(Ignore.Me, actual => Xunit.Assert.Empty((Actual ?? string.Empty).Trim())).And();

    /// <summary>
    /// Asserts that the string is upper case
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> UpperCase() => Assert(Ignore.Me, NotNullAnd(actual => Equal(actual, actual.ToUpper()))).And();

    /// <summary>
    /// Asserts that the string is lower case
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> LowerCase() => Assert(Ignore.Me, NotNullAnd(actual => Equal(actual, actual.ToLower()))).And();

    /// <summary>
    /// Asserts that the string is not the given value
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<IsStringContinuation> Not(
        string? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => NotEqual(actual, expected), 
            expectedExpr!).And();

    internal override ContinueWith<IsStringContinuation> Value(
        string expected, string expectedExpr)
        => Assert(() => Equal(expected, Actual), string.Empty, expectedExpr).And();

    internal override IsStringContinuation Continue() => Create(Actual, ActualExpr);
}