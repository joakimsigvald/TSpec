using System.Linq.Expressions;
using TSpec.Continuations;
using TSpec.Internal.Mocking;

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
        MockTarget<TService> target,
        Expression<Action<TService>> call,
        string callExpr)
        : base(spec, target, (setups, answer) => setups.AddVoid(call, answer), callExpr) { }

    private GivenThatVoidContinuation(
        Spec<TSUT, TResult> spec,
        Action<CallSetups, Func<IReadOnlyList<object>, object?>> answerCall,
        string member,
        Action guardArgumentReads)
        : base(spec, MockTarget<TService>.Family, answerCall, member, guardArgumentReads) { }

    /// Every call to a member of the name answering with nothing; the name is what the specification states.
    internal static GivenThatVoidContinuation<TSUT, TResult, TService> ByName(Spec<TSUT, TResult> spec, string member)
        => new(
            spec,
            (setups, answer) => setups.AddByName(typeof(TService), member, NothingToAnswer, answer),
            member,
            () => NamedMembers.AssertOne(typeof(TService), member, NothingToAnswer));

    private static Type NothingToAnswer => typeof(Continuations.Void);
}
