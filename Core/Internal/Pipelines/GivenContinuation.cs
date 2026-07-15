using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal class GivenContinuation<TSUT, TResult> : IGivenContinuation<TSUT, TResult>
{
    private readonly Spec<TSUT, TResult> _spec;

    internal GivenContinuation(Spec<TSUT, TResult> spec) => _spec = spec;

    public IGivenTestPipeline<TSUT, TResult> That(
        Action customArrangement,
        [CallerArgumentExpression(nameof(customArrangement))] string? customArrangementExpr = null)
        => _spec.GivenThat(customArrangement, customArrangementExpr!);

    public IGivenTestPipeline<TSUT, TResult> A<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.A(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> A<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.A(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> A<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.A(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> An<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.A(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> An<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.An(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> An<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.An(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFirst<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFirst(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFirst<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.AFirst(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFirst<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFirst(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.ASecond(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.ASecond(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> ASecond<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.ASecond(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.AThird(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.AThird(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> AThird<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.AThird(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFourth(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.AFourth(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFourth<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFourth(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        TValue value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFifth(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
         where TValue : class
        => _spec.Apply<TValue>(() => _spec.AFifth(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> AFifth<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _spec.Apply<TValue>(() => _spec.AFifth(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Zero<TValue>()
        => ApplyAsDefault(_spec.Zero<TValue>);

    public IGivenTestPipeline<TSUT, TResult> One<TValue>()
        => ApplyAsDefault(_spec.One<TValue>);

    public IGivenTestPipeline<TSUT, TResult> Two<TValue>()
        => ApplyAsDefault(_spec.Two<TValue>);

    public IGivenTestPipeline<TSUT, TResult> Three<TValue>()
        => ApplyAsDefault(_spec.Three<TValue>);

    public IGivenTestPipeline<TSUT, TResult> Four<TValue>()
        => ApplyAsDefault(_spec.Four<TValue>);

    public IGivenTestPipeline<TSUT, TResult> Five<TValue>()
        => ApplyAsDefault(_spec.Five<TValue>);

    public IGivenTestPipeline<TSUT, TResult> One<TValue>(
        TValue value, [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => ApplyAsDefault(() => _spec.One(value), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> Some<TValue>(
        IEnumerable<TValue> values, [CallerArgumentExpression(nameof(values))] string? valueExpr = null)
        => ApplyAsDefault(() => _spec.Some(values), valueExpr!);

    public IGivenTestPipeline<TSUT, TResult> One<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.One(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> One<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.One(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Two(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Two(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Two(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Two<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Two(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Three(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Three(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Three(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Three<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Three(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Four(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Four(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Four(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Four<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Four(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Action<TValue, int> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Five(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        => ApplyAsDefault(() => _spec.Five(setup), setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Five(transform), transformExpr!);

    public IGivenTestPipeline<TSUT, TResult> Five<TValue>(
        Func<TValue, int, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => ApplyAsDefault(() => _spec.Five(transform), transformExpr!);

    private IGivenTestPipeline<TSUT, TResult> ApplyAsDefault<TValue>(
        Func<TValue[]> getValues, string transformExpr,
        [CallerMemberName] string? article = null)
        => _spec.Apply<TValue>(() => _spec.Given(getValues(), string.Empty), transformExpr!, article: article);

    private IGivenTestPipeline<TSUT, TResult> ApplyAsDefault<TValue>(
        Func<TValue[]> getValues,
        Ignore ignore = default,
        [CallerMemberName] string? count = null)
        => _spec.ApplyMany<TValue>(() => _spec.Given(getValues(), string.Empty), count);
}