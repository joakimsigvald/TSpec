using System.Runtime.CompilerServices;
using TSpec.Assert;
using TSpec.Assert.Continuations;
using TSpec.Assert.Continuations.String;
using TSpec.Internal.Specification;

namespace TSpec;

/// <summary>
/// The generated specification text of a test. Comparisons on this type are line-ending-agnostic:
/// both the specification and the expected value are normalized before comparison, so expectations
/// match regardless of the source file's line endings and the platform the test runs on.
/// </summary>
public sealed class SpecificationText
{
    private readonly Lazy<string> _text;

    internal SpecificationText(Lazy<string> text) => _text = text;

    /// <summary>
    /// Verify that the specification is same as expected, ignoring line-ending differences
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the specification</returns>
    public ContinueWith<IsStringContinuation> Is(
        string? expected,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => _text.Value.NormalizeLineEndings()
            .Is(expected?.NormalizeLineEndings(), _actualExpr, expectedExpr);

    /// <summary>
    /// Get available assertions for the specification, normalized to '\n' line endings
    /// </summary>
    /// <returns>A continuation for making further assertions on the specification</returns>
    public IsString Is() => _text.Value.NormalizeLineEndings().Is(actualExpr: _actualExpr);

    /// <summary>
    /// Get available assertions for the characteristics of the specification, normalized to '\n' line endings
    /// </summary>
    /// <returns>A continuation for making further assertions on the specification</returns>
    public DoesString Does() => _text.Value.NormalizeLineEndings().Does(actualExpr: _actualExpr);

    /// <summary>
    /// Get the specification text as a string
    /// </summary>
    /// <returns>The specification text, with platform-native line endings</returns>
    public override string ToString() => _text.Value;

    /// <summary>
    /// Convert to the specification text, with platform-native line endings
    /// </summary>
    /// <param name="specification">The specification to convert</param>
    public static implicit operator string(SpecificationText specification) => specification.ToString();

    private const string _actualExpr = "Specification";
}
