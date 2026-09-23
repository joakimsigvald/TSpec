using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

internal class GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>
    : GivenTestPipeline<TSUT, TResult>, IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    private readonly GivenServiceContinuation<TSUT, TResult, TService> _serviceContinuation;
    private readonly GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>? _previous;
    private readonly MockTarget<TService> _target;

    internal GivenThatReturnsContinuation(
        Spec<TSUT, TResult> spec,
        GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>? previous,
        MockTarget<TService> target)
        : base(spec)
    {
        _serviceContinuation = new(spec, target);
        _previous = previous;
        _target = target;
    }

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> AndNext()
    {
        if (_previous?._sequence is null)
            throw new SetupFailed("AndNext must be preceded by First, which starts a sequential mock setup: Given...That...First");
        return _previous.AndNext();
    }

    public IGivenTestPipeline<TSUT, TResult> AndReturnsDefault<TReturns2>(
        Func<TReturns2> value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
    {
        if (!_target.IsFamily)
            throw new SetupFailed(
                $"A default is set up for every {typeof(TService).Alias()}, not for {_target.Name} alone. "
                + $"Set it up with Given<{typeof(TService).Alias()}>().Returns(…)");

        return _serviceContinuation.Returns(value, valueExpr!);
    }

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, TReturns2>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => _serviceContinuation.That(call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, Task<TReturns2>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null) 
        => _serviceContinuation.That(call, callExpr!);
}