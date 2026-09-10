namespace TSpec.Internal.Pipelines;

internal class GivenThatNextContinuation<TSUT, TResult, TService, TReturns>
    : GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    internal GivenThatNextContinuation(
        Spec<TSUT, TResult> spec,
        Func<object> setup,
        string callExpr,
        string? tapExpr = null,
        Lazy<object>? lazyContinuation = null,
        MockCallSequence<TReturns>? sequence = null,
        Action<IReadOnlyList<object>>? stepTap = null)
        : base(spec, setup, callExpr, tapExpr, lazyContinuation, sequence, stepTap)
    {
    }
}
