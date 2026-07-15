using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// Continuation for setting up an expectation for a tag, such as associating it with a value
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TValue">The type of the value associated with the tag</typeparam>
public interface IGivenTag<TSUT, TResult, TValue>
{
    /// <summary>
    /// Associate a tag with a value, which can be referenced in the pipeline using 'The([tag])'
    /// </summary>
    /// <param name="value">The value to associate with the tag</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Is(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Apply setup to the value associated with the tag
    /// </summary>
    /// <param name="setup">An action applied to the value associated with the tag</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Has(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null);

    /// <summary>
    /// Apply transform to the value associated with the tag
    /// </summary>
    /// <param name="transform">A function transforming the value associated with the tag</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Has(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);
}