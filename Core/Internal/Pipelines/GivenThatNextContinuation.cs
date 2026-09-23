using TSpec.Internal.Mocking;

namespace TSpec.Internal.Pipelines;

internal class GivenThatNextContinuation<TSUT, TResult, TService, TReturns>
    : GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    internal GivenThatNextContinuation(
        Spec<TSUT, TResult> spec,
        MockTarget<TService> target,
        Action<Func<IReadOnlyList<object>, object?>> answerCall,
        string callExpr,
        IReadOnlyList<string>? tapExprs = null,
        MockCallSequence<TReturns>? sequence = null,
        Action<IReadOnlyList<object>>? tap = null)
        : base(spec, target, answerCall, callExpr, tapExprs, sequence, tap)
    {
    }
}
