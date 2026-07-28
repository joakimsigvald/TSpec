using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

internal class GivenTag<TSUT, TResult, TValue>(
    Spec<TSUT, TResult> spec, Tag<TValue> tag, string tagExpr)
    : IGivenTag<TSUT, TResult, TValue>
{
    private readonly string _name = tagExpr.AsTagName();

    public IGivenTestPipeline<TSUT, TResult> Is(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => spec.Apply<TValue>(() => spec.Assign(tag, value), $"{_name} is {valueExpr!.Describe()}", true);

    public IGivenTestPipeline<TSUT, TResult> Has(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => spec.Apply<TValue>(() => spec.Apply(tag, setup), $"{_name} has {setupExpr!.Describe()}", true);

    public IGivenTestPipeline<TSUT, TResult> Has(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => spec.Apply<TValue>(() => spec.Apply(tag, transform), $"{_name} has {transformExpr!.Describe()}", true);
}