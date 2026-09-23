using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Mocking;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// A call named by an expression, answering with a value. What it adds to the common vocabulary is
/// the answer computed from the arguments — which is a tap that keeps what it read, so it reaches a
/// sequence step on the same terms as any other tap.
/// </summary>
internal class GivenThatContinuation<TSUT, TResult, TService, TReturns, TActualReturns>
    : GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>,
    IGivenThatContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    internal GivenThatContinuation(
        Spec<TSUT, TResult> spec,
        MockTarget<TService> target,
        Expression<Func<TService, TActualReturns>> call,
        string callExpr)
        : base(spec, target, (setups, answer) => setups.Add(call, typeof(TReturns), answer), callExpr) { }

    /// A member named because no expression can name it; the name is what the specification states.
    internal GivenThatContinuation(Spec<TSUT, TResult> spec, string member)
        : base(
            spec,
            MockTarget<TService>.Family,
            (setups, answer) => setups.Add(
                ProtectedMember.Resolve<TService>(member, Answering), typeof(TReturns), answer),
            member) { }

    private static Type[] Answering =>
        [typeof(TReturns), typeof(Task<TReturns>), typeof(ValueTask<TReturns>)];

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg>(
        Func<TArg, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
        => Computed(returns, args => returns(Arg<TArg>(args, 0)), returnsExpr);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2>(
        Func<TArg1, TArg2, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
        => Computed(returns, args => returns(Arg<TArg1>(args, 0), Arg<TArg2>(args, 1)), returnsExpr);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3>(
        Func<TArg1, TArg2, TArg3, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
        => Computed(
            returns,
            args => returns(Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2)),
            returnsExpr);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4>(
        Func<TArg1, TArg2, TArg3, TArg4, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
        => Computed(
            returns,
            args => returns(
                Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2), Arg<TArg4>(args, 3)),
            returnsExpr);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
        => Computed(
            returns,
            args => returns(
                Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2), Arg<TArg4>(args, 3),
                Arg<TArg5>(args, 4)),
            returnsExpr);

    /// <summary>
    /// The answer is computed by a tap and held for the outcome, which the call's answer runs next.
    /// </summary>
    private IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Computed(
        Delegate returns, Func<IReadOnlyList<object>, TReturns> answer, string? returnsExpr)
    {
        if (returns is null)
            throw new SetupFailed("returns may not be null");
        TReturns? computed = default;
        return Observing(args => computed = answer(args)).Returns(() => computed, returnsExpr!);
    }
}
