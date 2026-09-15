using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData.Generation.Strategies.Mocking;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// What a mocked call does, whichever way the call was named. One answer stands behind every form:
/// a lone outcome is that answer, and a sequence queues its steps behind it. So the outcome
/// vocabulary — Returns, Throws, Tap — is stated once here and reads the same before First as after
/// it.
/// </summary>
internal abstract class GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    : IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    private readonly Spec<TSUT, TResult> _spec;
    private readonly Action<Func<IReadOnlyList<object>, object?>> _answerCall;
    private readonly string _callExpr;
    private readonly IReadOnlyList<string> _tapExprs;
    private readonly Action<IReadOnlyList<object>>? _tap;
    internal readonly MockCallSequence<TReturns>? _sequence;

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Action<MockHandle, Func<IReadOnlyList<object>, object?>> answerCall,
        string callExpr)
        : this(spec, answer => answerCall(spec.Pipeline.GetMock<TService>(), answer), callExpr)
    {
    }

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Action<Func<IReadOnlyList<object>, object?>> answerCall,
        string callExpr,
        IReadOnlyList<string>? tapExprs = null,
        MockCallSequence<TReturns>? sequence = null,
        Action<IReadOnlyList<object>>? tap = null)
    {
        _spec = spec;
        _answerCall = answerCall;
        _callExpr = callExpr;
        _tapExprs = tapExprs ?? [];
        _sequence = sequence;
        _tap = tap;
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns()
    {
        _spec.AppendGiven(SetupReturns);
        return Done();
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Func<TReturns?> returns, [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
    {
        _spec.AppendGiven(() => SetupReturns(returns!, returnsExpr!));
        return Done();
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Tag<TReturns?> tag, [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Returns(() => _spec.The(tag), tagExpr!.AsTagName());

    /// <summary>
    /// The default of nothing is nothing: where the call answers with no value there is no default
    /// to hand back. Where it answers with a task, the default is one that has completed, since a
    /// null task is not something an awaiting caller can be handed.
    /// </summary>
    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> ReturnsDefault()
        => typeof(TReturns) == typeof(Continuations.Void) ? Returns() : Returns(Nothing, "() => default");

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws<TException>()
        where TException : Exception, new()
    {
        _spec.AppendGiven(SetupThrows<TException>);
        return Done();
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        _spec.AppendGiven(() => SetupThrows(expected, expectedExpr!));
        return Done();
    }

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap(
        Action callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(_ => callback(), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg>(
        Action<TArg> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(args => callback(Arg<TArg>(args, 0)), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2>(
        Action<TArg1, TArg2> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(args => callback(Arg<TArg1>(args, 0), Arg<TArg2>(args, 1)), callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3>(
        Action<TArg1, TArg2, TArg3> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(
            args => callback(Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2)),
            callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4>(
        Action<TArg1, TArg2, TArg3, TArg4> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(
            args => callback(
                Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2), Arg<TArg4>(args, 3)),
            callbackExpr!);

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Action<TArg1, TArg2, TArg3, TArg4, TArg5> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null)
        => Tapping(
            args => callback(
                Arg<TArg1>(args, 0), Arg<TArg2>(args, 1), Arg<TArg3>(args, 2), Arg<TArg4>(args, 3),
                Arg<TArg5>(args, 4)),
            callbackExpr!);

    /// <summary>
    /// Opens a sequence over the call already in hand, rather than a call of its own: the steps are
    /// TSpec's, so the call keeps the one answer that a tap and an outcome both need. The taps in
    /// hand go to the sequence, which taps every call with them.
    /// </summary>
    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> First()
        => InSequence(_callExpr, new MockCallSequence<TReturns>(_tap, _tapExprs));

    internal GivenThatNextContinuation<TSUT, TResult, TService, TReturns> AndNext()
        => InSequence("next", _sequence!);

    /// A tap the specification does not state, for a step that reads the call to answer it.
    protected IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Observing(
        Action<IReadOnlyList<object>> observe)
        => Tapping(observe, tapExpr: null);

    protected static TArg Arg<TArg>(IReadOnlyList<object> arguments, int index)
        => (TArg)arguments[index];

    private GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Done() => new(_spec, this);

    /// <summary>
    /// A tap inside a sequence belongs to the step it precedes, so it fires on the call that step
    /// answers and no other; outside one it fires on every call. Either way it is folded into the
    /// taps already in hand rather than replacing them, since a second tap is a second observation
    /// of the call and not a correction of the first.
    /// </summary>
    private IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tapping(
        Action<IReadOnlyList<object>> tap, string? tapExpr)
        => new GivenThatNextContinuation<TSUT, TResult, TService, TReturns>(
            _spec,
            _answerCall,
            _callExpr,
            tapExpr is null ? _tapExprs : [.. _tapExprs, tapExpr],
            _sequence,
            Then(_tap, tap));

    private static Action<IReadOnlyList<object>> Then(
        Action<IReadOnlyList<object>>? first, Action<IReadOnlyList<object>> next)
        => first is null ? next : args => { first(args); next(args); };

    private GivenThatNextContinuation<TSUT, TResult, TService, TReturns> InSequence(
        string callExpr, MockCallSequence<TReturns> sequence)
        => new(_spec, _answerCall, callExpr, sequence: sequence);

    /// <summary>
    /// The outcome, preceded by the taps in hand. Outside a sequence it answers the call; inside one
    /// it is queued as a step, and the first step points the call at the queue.
    /// </summary>
    private void Answer(Func<IReadOnlyList<object>, TReturns?> outcome)
    {
        var tap = _tap;
        Func<IReadOnlyList<object>, TReturns?> step = tap is null
            ? outcome
            : args =>
            {
                tap(args);
                return outcome(args);
            };
        if (_sequence is null)
        {
            _answerCall(args => step(args));
            return;
        }

        var sequence = _sequence;
        var opens = sequence.IsEmpty;
        sequence.Append(step);
        if (opens)
            _answerCall(args => sequence.Next(args));
    }

    private void SetupReturns()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockReturns();
        if (_sequence is null && !AnswersNothing)
            throw new SetupFailed(
                $"Cannot apply Returns to '{_callExpr}': it answers with {typeof(TReturns).Alias()}, "
                + "so state what it returns");
        Answer(_ => Nothing());
    }

    private static bool AnswersNothing
        => typeof(TReturns) == typeof(Continuations.Void)
        || typeof(TReturns) == typeof(Task)
        || typeof(TReturns) == typeof(ValueTask);

    /// <summary>
    /// What a step answers when it was told to return nothing. Where the call is awaited, nothing
    /// is a completed task rather than the type's default — a null task is not something an awaiting
    /// caller can be handed.
    /// </summary>
    private static TReturns? Nothing()
        => typeof(TReturns) == typeof(Task) ? (TReturns)(object)Task.CompletedTask
        : typeof(TReturns) == typeof(ValueTask) ? (TReturns)(object)default(ValueTask)
        : default;

    private void SetupReturns(Func<TReturns?> returns, string returnsExpr)
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockReturns(returnsExpr);
        if (_sequence is null && typeof(TReturns) == typeof(Continuations.Void))
            throw new SetupFailed($"Cannot apply Returns to '{_callExpr}': it answers with nothing");
        Answer(_ => returns());
    }

    private void SetupThrows<TException>()
        where TException : Exception, new()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows<TException>();
        Answer(_ => throw new TException());
    }

    /// A lone outcome throws the exception made as it is set up; each step of a sequence makes its own.
    private void SetupThrows(Func<Exception> expected, string expectedExpr)
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows(expectedExpr);
        if (_sequence is not null)
        {
            Answer(_ => throw expected());
            return;
        }

        var exception = expected();
        Answer(_ => throw exception);
    }

    /// The step that opens a sequence states the sequence's own taps, then that it comes first.
    private void SpecifyMock()
    {
        if (_callExpr is not null)
            _spec.Pipeline.Specification.AddMockSetup<TService>(_callExpr);
        if (_sequence is { IsEmpty: true })
        {
            foreach (var tapExpr in _sequence.TapExprs)
                _spec.Pipeline.Specification.AddTap(tapExpr);
            _spec.Pipeline.Specification.AddMockFirst();
        }
        foreach (var tapExpr in _tapExprs)
            _spec.Pipeline.Specification.AddTap(tapExpr);
    }
}
