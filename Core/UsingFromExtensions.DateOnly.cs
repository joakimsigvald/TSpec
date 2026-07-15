using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.TestData.Generation;

namespace TSpec;

public static partial class UsingFromExtensions
{
    /// <summary>
    /// Instructs the generator to produce source values starting at the given value.
    /// Unless a spacing is provided with Spaced, consecutive values are one day apart.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="start">The first value to generate.</param>
    /// <param name="startExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, DateOnly> StartingAt<TSUT, TResult>(
        this IUsingFromContinuation<TSUT, TResult, DateOnly> pipeline,
        DateOnly start,
        [CallerArgumentExpression(nameof(start))] string? startExpr = null)
    {
        Sequence<DateOnlySequence>(pipeline).SetStart(start, startExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values with the given spacing in days between consecutive values.
    /// A negative spacing yields a descending sequence.
    /// Unless a start is provided with StartingAt, the sequence starts at the data generator's epoch date.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="days">The number of days between consecutive values. Cannot be zero.</param>
    /// <param name="daysExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, DateOnly> Spaced<TSUT, TResult>(
        this IUsingFromContinuation<TSUT, TResult, DateOnly> pipeline,
        int days,
        [CallerArgumentExpression(nameof(days))] string? daysExpr = null)
    {
        if (days == 0)
            throw new SetupFailed("Spacing cannot be zero");
        Sequence<DateOnlySequence>(pipeline).SetInterval(current => current.AddDays(days), daysExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values by applying the given step function to the previous value.
    /// Unless a start is provided with StartingAt, the sequence starts at the data generator's epoch date.
    /// The step function may produce any sequence, as long as it never repeats a value.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="spacing">A function computing the next value from the previous one.</param>
    /// <param name="spacingExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, DateOnly> Spaced<TSUT, TResult>(
        this IUsingFromContinuation<TSUT, TResult, DateOnly> pipeline,
        Func<DateOnly, DateOnly> spacing,
        [CallerArgumentExpression(nameof(spacing))] string? spacingExpr = null)
    {
        if (spacing is null)
            throw new SetupFailed("Spacing cannot be null");
        Sequence<DateOnlySequence>(pipeline).SetStep((current, _) => spacing(current), spacingExpr!);
        return pipeline;
    }

    /// <summary>
    /// Instructs the generator to produce source values by applying the given step function
    /// to the previous value and the position of the value being generated (the start value has position 0).
    /// Unless a start is provided with StartingAt, the sequence starts at the data generator's epoch date.
    /// The step function may produce any sequence, as long as it never repeats a value.
    /// </summary>
    /// <param name="pipeline">The From-continuation whose source values to constrain.</param>
    /// <param name="spacing">A function computing the next value from the previous value and the position of the next value.</param>
    /// <param name="spacingExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The same continuation, for further sequence constraints or arrangement.</returns>
    public static IUsingFromContinuation<TSUT, TResult, DateOnly> Spaced<TSUT, TResult>(
        this IUsingFromContinuation<TSUT, TResult, DateOnly> pipeline,
        Func<DateOnly, int, DateOnly> spacing,
        [CallerArgumentExpression(nameof(spacing))] string? spacingExpr = null)
    {
        if (spacing is null)
            throw new SetupFailed("Spacing cannot be null");
        Sequence<DateOnlySequence>(pipeline).SetStep(spacing, spacingExpr!);
        return pipeline;
    }
}
