namespace TSpec.Internal.Pipelines;

internal class GivenThatNextContinuation<TSUT, TResult, TService, TReturns>
    : GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    internal GivenThatNextContinuation(
        Spec<TSUT, TResult> spec,
        Func<bool, object> setup,
        string callExpr,
        string? tapExpr = null,
        Lazy<object>? lazyContinuation = null,
        bool isSequential = false)
        : base(spec, setup, callExpr, tapExpr, lazyContinuation, isSequential)
    {
    }
}