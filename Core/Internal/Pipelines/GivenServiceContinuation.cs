using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Mocking;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

internal class GivenServiceContinuation<TSUT, TResult, TService> : IGivenServiceContinuation<TSUT, TResult, TService>
    where TService : class
{
    private readonly Spec<TSUT, TResult> _spec;
    private readonly MockTarget<TService> _target;

    internal GivenServiceContinuation(Spec<TSUT, TResult> spec, MockTarget<TService> target)
    {
        _spec = spec;
        _target = target;
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Func<TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
    {
        _spec.AppendGiven(DoSetupReturnsDefault);
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, null, _target);

        void DoSetupReturnsDefault()
        {
            var theValue = returns();
            _spec.SetupReturnsDefault<TService, TReturns>(theValue);
            _spec.Pipeline.Specification.AddMockReturnsDefault<TService>(returnsExpr!);
        }
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Tag<TReturns> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Returns(() => _spec.The(tag), tagExpr!.AsTagName());

    public IGivenTestPipeline<TSUT, TResult> Throws<TException>() where TException : Exception
    {
        _spec.Pipeline.Specification.AddMockThrowsDefault<TService, TException>();
        _spec.SetupThrows<TService>(_spec.Any<TException>);
        return new GivenTestPipeline<TSUT, TResult>(_spec);
    }

    public IGivenTestPipeline<TSUT, TResult> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        _spec.Pipeline.Specification.AddMockThrowsDefault<TService>(expectedExpr!);
        _spec.SetupThrows<TService>(expected);
        return new GivenTestPipeline<TSUT, TResult>(_spec);
    }

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(string member)
        => GivenThatContinuation<TSUT, TResult, TService, TReturns, TReturns>.ByName(_spec, member);

    public IGivenThatVoidContinuation<TSUT, TResult, TService> That(string member)
        => GivenThatVoidContinuation<TSUT, TResult, TService>.ByName(_spec, member);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> ThatProtected<TReturns>(string member)
        => That<TReturns>(member);

    public IGivenThatVoidContinuation<TSUT, TResult, TService> ThatProtected(string member)
        => That(member);

    public IGivenThatVoidContinuation<TSUT, TResult, TService> That(
        Expression<Action<TService>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatVoidContinuation<TSUT, TResult, TService>(_spec, _target, call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, TReturns>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatContinuation<TSUT, TResult, TService, TReturns, TReturns>(_spec, _target, call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, Task<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatContinuation<TSUT, TResult, TService, TReturns, Task<TReturns>>(_spec, _target, call, callExpr!);

    public IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, ValueTask<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        => new GivenThatContinuation<TSUT, TResult, TService, TReturns, ValueTask<TReturns>>(_spec, _target, call, callExpr!);
}