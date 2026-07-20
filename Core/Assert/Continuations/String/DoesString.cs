using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Object that allows assertions to be made on the provided string
/// </summary>
public record DoesString : StringConstraint<DoesStringContinuation>
{
    /// <summary>
    /// Asserts that the string contains the expected string
    /// </summary>
    /// <param name="expected">The substring that the string is expected to contain</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> Contain(
        string? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => Xunit.Assert.Contains(expected!, actual),
            expectedExpr!,
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string contains the expected string, compared with the given comparison
    /// </summary>
    /// <param name="expected">The substring that the string is expected to contain</param>
    /// <param name="comparison">How to compare the strings</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> Contain(
        string? expected, StringComparison comparison,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            $"{Describe(expected)}{DescribeComparison(comparison)}",
            actual => Xunit.Assert.Contains(expected!, actual, comparison),
            $"{expectedExpr}{DescribeComparison(comparison)}",
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string matches the given regular expression pattern
    /// </summary>
    /// <param name="pattern">The regular expression pattern that the string is expected to match</param>
    /// <param name="patternExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> Match(
        string pattern, [CallerArgumentExpression(nameof(pattern))] string? patternExpr = null)
        => Assert(
            Describe(pattern),
            actual => Xunit.Assert.Matches(pattern, actual),
            patternExpr!,
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string matches the given regular expression
    /// </summary>
    /// <param name="regex">The regular expression that the string is expected to match, e.g. with custom options</param>
    /// <param name="regexExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> Match(
        Regex regex, [CallerArgumentExpression(nameof(regex))] string? regexExpr = null)
        => Assert(
            Describe(regex.ToString()),
            actual => Xunit.Assert.Matches(regex, actual),
            regexExpr!,
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string starts with a prefix
    /// </summary>
    /// <param name="expected">The prefix that the string is expected to start with</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> StartWith(
        string? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => Xunit.Assert.StartsWith(expected!, actual),
            expectedExpr!,
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string starts with a prefix, compared with the given comparison
    /// </summary>
    /// <param name="expected">The prefix that the string is expected to start with</param>
    /// <param name="comparison">How to compare the strings</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> StartWith(
        string? expected, StringComparison comparison,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            $"{Describe(expected)}{DescribeComparison(comparison)}",
            actual => Xunit.Assert.StartsWith(expected!, actual, comparison),
            $"{expectedExpr}{DescribeComparison(comparison)}",
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS).And();

    /// <summary>
    /// Asserts that the string ends with a suffix
    /// </summary>
    /// <param name="expected">The suffix that the string is expected to end with</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> EndWith(
        string? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            Describe(expected),
            actual => Xunit.Assert.EndsWith(expected!, actual),
            expectedExpr!,
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS)
        .And();

    /// <summary>
    /// Asserts that the string ends with a suffix, compared with the given comparison
    /// </summary>
    /// <param name="expected">The suffix that the string is expected to end with</param>
    /// <param name="comparison">How to compare the strings</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<DoesStringContinuation> EndWith(
        string? expected, StringComparison comparison,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => Assert(
            $"{Describe(expected)}{DescribeComparison(comparison)}",
            actual => Xunit.Assert.EndsWith(expected!, actual, comparison),
            $"{expectedExpr}{DescribeComparison(comparison)}",
            verbalizationStrategy: VerbalizationStrategy.PresentSingularS)
        .And();

    private static string DescribeComparison(StringComparison comparison)
        => comparison switch
        {
            StringComparison.OrdinalIgnoreCase => " ignoring case",
            StringComparison.CurrentCulture => " using current culture",
            StringComparison.CurrentCultureIgnoreCase => " using current culture ignoring case",
            StringComparison.InvariantCulture => " using invariant culture",
            StringComparison.InvariantCultureIgnoreCase => " using invariant culture ignoring case",
            _ => string.Empty,
        };

    internal override DoesStringContinuation Continue() => Create(Actual, ActualExpr);
}