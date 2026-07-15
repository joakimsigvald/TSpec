using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal class GivenServiceContinuation<TSUT, TResult, TService> : IGivenServiceContinuation<TSUT, TResult, TService>
    where TService : class
{
    private readonly Spec<TSUT, TResult> _spec;

    internal GivenServiceContinuation(Spec<TSUT, TResult> spec) => _spec = spec;

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Func<TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
    {
        _spec.AppendGiven(DoSetupReturnsDefault);
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, null);

        void DoSetupReturnsDefault()
        {
            var theValue = returns();
            _spec.GetMock<TService>().SetReturnsDefault(theValue);
            _spec.GetMock<TService>().SetReturnsDefault(Task.FromResult(theValue));
            _spec.Pipeline.Specification.AddMockReturnsDefault<TService>(returnsExpr!);
        }
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Tag<TReturns> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Returns(() => _spec.The(tag), tagExpr);

    public IGivenTestPipeline<TSUT, TResult> Throws<TException>() where TException : Exception
    {
        _spec.Pipeline.Specification.AddMockThrowsDefault<TService, TException>();
        _spec.SetupThrows<TService>(_spec.Another<TException>);
        return new GivenTestPipeline<TSUT, TResult>(_spec);
    }

    public IGivenTestPipeline<TSUT, TResult> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        _spec.Pipeline.Specification.AddMockThrowsDefault<TService>(expectedExpr!);
        _spec.SetupThrows<TService>(expected);
        return new GivenTestPipeline<TSUT, TResult>(_spec);
    }

    public IGivenThatVoidContinuation<TSUT, TResult, TService> That(
        Expression<Action<TService>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatVoidContinuation<TSUT, TResult, TService>(_spec, call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, TReturns>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatContinuation<TSUT, TResult, TService, TReturns, TReturns>(_spec, call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, Task<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatContinuation<TSUT, TResult, TService, TReturns, Task<TReturns>>(_spec, call, callExpr!);
}