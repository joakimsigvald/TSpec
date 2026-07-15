using System.Numerics;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.TestData.Generation;

namespace TSpec;

public static partial class UsingFromExtensions
{
    /// <summary>
    /// Instructs the generator to produce source values starting at the given value.
    /// Unless a spacing is provided with Spaced, consecutive values are one apart.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="start">The first value to generate.</param>
    /// <param name="startExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, TSource> StartingAt<TSUT, TResult, TSource>(
        this IUsingFromContinuation<TSUT, TResult, TSource> pipeline,
        TSource start,
        [CallerArgumentExpression(nameof(start))] string? startExpr = null)
        where TSource : INumber<TSource>
    {
        Sequence<NumericSequence<TSource>>(pipeline).SetStart(start, startExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values with the given spacing between consecutive values.
    /// A negative spacing yields a descending sequence. Unless a start is provided with StartingAt, the sequence starts at one.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="spacing">The difference between consecutive values. Cannot be zero.</param>
    /// <param name="spacingExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, TSource> Spaced<TSUT, TResult, TSource>(
        this IUsingFromContinuation<TSUT, TResult, TSource> pipeline,
        TSource spacing,
        [CallerArgumentExpression(nameof(spacing))] string? spacingExpr = null)
        where TSource : INumber<TSource>
    {
        if (TSource.IsZero(spacing))
            throw new SetupFailed("Spacing cannot be zero");
        Sequence<NumericSequence<TSource>>(pipeline).SetInterval(current => current + spacing, spacingExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values by applying the given step function to the previous value.
    /// Unless a start is provided with StartingAt, the sequence starts at one.
    /// The step function may produce any sequence, as long as it never repeats a value.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="spacing">A function computing the next value from the previous one.</param>
    /// <param name="spacingExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, TSource> Spaced<TSUT, TResult, TSource>(
        this IUsingFromContinuation<TSUT, TResult, TSource> pipeline,
        Func<TSource, TSource> spacing,
        [CallerArgumentExpression(nameof(spacing))] string? spacingExpr = null)
        where TSource : INumber<TSource>
    {
        if (spacing is null)
            throw new SetupFailed("Spacing cannot be null");
        Sequence<NumericSequence<TSource>>(pipeline).SetStep((current, _) => spacing(current), spacingExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values by applying the given step function
    /// to the previous value and the position of the value being generated (the start value has position 0),
    /// e.g. StartingAt(0).Spaced((_, i) => i * i) yields 0, 1, 4, 9, ...
    /// Unless a start is provided with StartingAt, the sequence starts at one.
    /// The step function may produce any sequence, as long as it never repeats a value.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="spacing">A function computing the next value from the previous value and the position of the next value.</param>
    /// <param name="spacingExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, TSource> Spaced<TSUT, TResult, TSource>(
        this IUsingFromContinuation<TSUT, TResult, TSource> pipeline,
        Func<TSource, int, TSource> spacing,
        [CallerArgumentExpression(nameof(spacing))] string? spacingExpr = null)
        where TSource : INumber<TSource>
    {
        if (spacing is null)
            throw new SetupFailed("Spacing cannot be null");
        Sequence<NumericSequence<TSource>>(pipeline).SetStep(spacing, spacingExpr!);
        return pipeline;
    }
}
