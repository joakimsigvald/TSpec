using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal class GivenThatVoidContinuation<TSUT, TResult, TService>
    : GivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void>,
    IGivenThatVoidContinuation<TSUT, TResult, TService>
    where TService : class
{
    internal GivenThatVoidContinuation(
        Spec<TSUT, TResult> spec,
        Expression<Action<TService>> call,
        string callExpr)
        : base(spec, GetSetup(call), callExpr) {}

    private static Func<Mock<TService>, bool, IFluentInterface> GetSetup(Expression<Action<TService>> call)
        => (mock, isSequential) => isSequential ? (IFluentInterface)mock.SetupSequence(call) : mock.Setup(call);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap(
        Action callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap<TArg>(
        Action<TArg> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap<TArg1, TArg2>(
        Action<TArg1, TArg2> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap<TArg1, TArg2, TArg3>(
        Action<TArg1, TArg2, TArg3> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap<TArg1, TArg2, TArg3, TArg4>(
        Action<TArg1, TArg2, TArg3, TArg4> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void> Tap<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Action<TArg1, TArg2, TArg3, TArg4, TArg5> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => ContinueWith(() => MockSetup.Callback(callback), callbackExpr!);

    private Moq.Language.Flow.ISetup<TService> MockSetup
        => (Moq.Language.Flow.ISetup<TService>)Continuation;

}