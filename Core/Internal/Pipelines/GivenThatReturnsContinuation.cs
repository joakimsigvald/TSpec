using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal class GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>
    : GivenTestPipeline<TSUT, TResult>, IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    private readonly GivenServiceContinuation<TSUT, TResult, TService> _serviceContinuation;
    private readonly GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>? _previous;

    internal GivenThatReturnsContinuation(
        Spec<TSUT, TResult> spec, GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>? previous) 
        : base(spec)
    {
        _serviceContinuation = new(spec);
        _previous = previous;
    }

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> AndNext()
    {
        if (_previous is null || !_previous._isSequential)
            throw new SetupFailed("AndNext must be preceded by First, which starts a sequential mock setup: Given...That...First");
        return _previous.AndNext();
    }

    public IGivenTestPipeline<TSUT, TResult> AndReturnsDefault<TReturns2>(Func<TReturns2> value)
        => _serviceContinuation.Returns(value);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, TReturns2>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => _serviceContinuation.That(call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, Task<TReturns2>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null) 
        => _serviceContinuation.That(call, callExpr!);
}