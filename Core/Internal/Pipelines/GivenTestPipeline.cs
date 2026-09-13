using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal class GivenTestPipeline<TSUT, TResult>
    : TestPipeline<TSUT, TResult, Spec<TSUT, TResult>>, IGivenTestPipeline<TSUT, TResult>
{
    internal GivenTestPipeline(Spec<TSUT, TResult> parent) : base(parent) { }

    public IGivenTag<TSUT, TResult, TValue> And<TValue>(
        Tag<TValue> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => new GivenTag<TSUT, TResult, TValue>(_parent, tag, tagExpr!);

    public IGivenServiceContinuation<TSUT, TResult, TService> And<TService>() where TService : class => Given<TService>();
    public IGivenContinuation<TSUT, TResult> And() => Given();
    public IGivenContinuation<TSUT, TResult> and => Given();
}