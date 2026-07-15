using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

internal class GivenTag<TSUT, TResult, TValue>(
    Spec<TSUT, TResult> spec, Tag<TValue> tag, string tagExpr)
    : IGivenTag<TSUT, TResult, TValue>
{
    public IGivenTestPipeline<TSUT, TResult> Is(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => spec.Apply<TValue>(() => spec.Assign(tag, value, tagExpr), $"{tagExpr} is {valueExpr!.ParseValue()}", true);

    public IGivenTestPipeline<TSUT, TResult> Has(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => spec.Apply<TValue>(() => spec.Apply(tag, setup, tagExpr), $"{tagExpr} has {setupExpr!.ParseValue()}", true);

    public IGivenTestPipeline<TSUT, TResult> Has(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => spec.Apply<TValue>(() => spec.Apply(tag, transform, tagExpr), $"{tagExpr} has {transformExpr!.ParseValue()}", true);
}