using Moq;
using System.Linq.Expressions;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// A call named by an expression, answering with nothing. It adds no vocabulary of its own: a void
/// call is tapped and sequenced exactly as a value-returning one is.
/// </summary>
internal class GivenThatVoidContinuation<TSUT, TResult, TService>
    : GivenThatCommonContinuation<TSUT, TResult, TService, Continuations.Void>,
    IGivenThatVoidContinuation<TSUT, TResult, TService>
    where TService : class
{
    internal GivenThatVoidContinuation(
        Spec<TSUT, TResult> spec,
        Expression<Action<TService>> call,
        string callExpr)
        : base(spec, GetSetup(AnyArgument.Rewrite(call)), callExpr) { }

    private static Func<Mock<TService>, object> GetSetup(Expression<Action<TService>> call)
        => mock => mock.Setup(call);
}
