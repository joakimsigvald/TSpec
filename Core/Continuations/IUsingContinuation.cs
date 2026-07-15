using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation returned by Using&lt;TTarget&gt;(), allowing the source of the target type's values to be specified with From.
/// If From is not called, the target type is instead registered as a concrete class to instantiate when the subject under test requires an abstraction it implements.
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TTarget">The target type whose value source is being specified</typeparam>
/// <example>
/// Generate CustomerId values by generating ints and converting them:
/// <code>
/// Using&lt;CustomerId&gt;().From&lt;int&gt;(id =&gt; new CustomerId(id))
/// </code>
/// or provide an explicit value space:
/// <code>
/// Using&lt;string&gt;().From(["SEK", "USD", "EUR"])
/// </code>
/// </example>
public interface IUsingContinuation<TSUT, TResult, TTarget> : IUsingTestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Registers a type conversion strategy. Whenever the target type is requested, the generator will first generate the source type and cast it.
    /// </summary>
    /// <typeparam name="TSource">The underlying primitive or source type to generate first.</typeparam>
    /// <returns>A continuation to constrain the generated source values with StartingAt or Spaced, or provide further arrangement.</returns>
    IUsingFromContinuation<TSUT, TResult, TSource> From<TSource>();

    /// <summary>
    /// Registers a type conversion strategy with a specific conversion function. Whenever the target type is requested, the generator will generate the source type and apply the conversion.
    /// </summary>
    /// <typeparam name="TSource">The underlying primitive or source type to generate first.</typeparam>
    /// <param name="convert">The function used to convert the source type into the target type.</param>
    /// <returns>A continuation to constrain the generated source values with StartingAt or Spaced, or provide further arrangement.</returns>
    IUsingFromContinuation<TSUT, TResult, TSource> From<TSource>(Func<TSource, TTarget> convert);

    /// <summary>
    /// Registers a generator function describing the value space of the target type.
    /// Whenever the target type is requested, the generator produces the next source value, which is converted to the target type if necessary.
    /// The values are used exactly as produced: the user defines the value space, so duplicates are allowed.
    /// </summary>
    /// <typeparam name="TSource">The type of the generated values.</typeparam>
    /// <param name="generate">A function producing the next value of the space, invoked once per generated value.</param>
    /// <param name="generateExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    IUsingTestPipeline<TSUT, TResult> From<TSource>(
        Func<TSource> generate,
        [CallerArgumentExpression(nameof(generate))] string? generateExpr = null);

    /// <summary>
    /// Registers an explicit list of values as the value space of the target type.
    /// Whenever the target type is requested, the next value from the list is used, converted to the target type if necessary.
    /// The values are used in declaration order, exactly as provided: the user defines the value space, so duplicates are allowed.
    /// Requesting more values than the list contains throws ValuesExhausted.
    /// </summary>
    /// <typeparam name="TSource">The type of the provided values.</typeparam>
    /// <param name="values">The values of the space, used in declaration order.</param>
    /// <param name="valuesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    IUsingTestPipeline<TSUT, TResult> From<TSource>(
        TSource[] values,
        [CallerArgumentExpression(nameof(values))] string? valuesExpr = null);
}
