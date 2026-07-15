using System.Runtime.CompilerServices;
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

    internal override DoesStringContinuation Continue() => Create(Actual, ActualExpr);
}