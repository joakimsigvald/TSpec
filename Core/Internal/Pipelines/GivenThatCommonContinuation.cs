using Moq;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// What a mocked call does, whichever way the call was named. One ordinary Moq setup stands behind
/// every form: a lone outcome installs itself on that setup, and a sequence queues its steps behind
/// it. So the outcome vocabulary — Returns, Throws, Tap — is stated once here and reads the same
/// before First as after it.
/// </summary>
internal abstract class GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    : IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    private readonly Spec<TSUT, TResult> _spec;
    private readonly Lazy<object> _lazyContinuation;
    private readonly Func<object> _setup;
    private readonly string _callExpr;
    private readonly IReadOnlyList<string> _tapExprs;
    private readonly Action<IReadOnlyList<object>>? _stepTap;
    internal readonly MockCallSequence<TReturns>? _sequence;

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Func<Mock<TService>, object> setup,
        string callExpr)
        : this(spec, () => setup(spec.GetMock<TService>()), callExpr)
    {
    }

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Func<object> setup,
        string callExpr,
        IReadOnlyList<string>? tapExprs = null,
        Lazy<object>? lazyContinuation = null,
        MockCallSequence<TReturns>? sequence = null,
        Action<IReadOnlyList<object>>? stepTap = null)
    {
        _spec = spec;
        _setup = setup;
        _callExpr = callExpr;
        _tapExprs = tapExprs ?? [];
        _lazyContinuation = lazyContinuation ?? new Lazy<object>(DoSetup);
        _sequence = sequence;
        _stepTap = stepTap;
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

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> ReturnsDefault()
        => Returns(() => default);

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
    /// Opens a sequence over the setup already in hand, rather than a setup of its own: the steps
    /// are TSpec's, so the call keeps the one Moq setup that a tap and an outcome both need.
    /// </summary>
    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> First()
        => InSequence(_callExpr + " first", new MockCallSequence<TReturns>());

    internal GivenThatNextContinuation<TSUT, TResult, TService, TReturns> AndNext()
        => InSequence("next", _sequence!);

    protected GivenThatNextContinuation<TSUT, TResult, TService, TReturns> ContinueWith(
        Func<object> callback,
        IReadOnlyList<string>? tapExprs = null,
        Action<IReadOnlyList<object>>? stepTap = null)
        => new(_spec, callback, _callExpr, tapExprs, sequence: _sequence, stepTap: stepTap ?? _stepTap);

    protected object Continuation => _lazyContinuation.Value;

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
    /// of the call and not a correction of the first — and Moq keeps only one callback per setup,
    /// so what reaches it has to be the fold.
    /// </summary>
    private IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tapping(
        Action<IReadOnlyList<object>> tap, string? tapExpr)
    {
        var observed = Then(_stepTap, tap);
        var stated = tapExpr is null ? _tapExprs : [.. _tapExprs, tapExpr];
        return _sequence is null
            ? ContinueWith(() => Capturing(observed), stated, observed)
            : new GivenThatNextContinuation<TSUT, TResult, TService, TReturns>(
                _spec, _setup, _callExpr, stated, _lazyContinuation, _sequence, observed);
    }

    private static Action<IReadOnlyList<object>> Then(
        Action<IReadOnlyList<object>>? first, Action<IReadOnlyList<object>> next)
        => first is null ? next : args => { first(args); next(args); };

    private GivenThatNextContinuation<TSUT, TResult, TService, TReturns> InSequence(
        string callExpr, MockCallSequence<TReturns> sequence)
        => new(_spec, _setup, callExpr, lazyContinuation: _lazyContinuation, sequence: sequence);

    private object DoSetup() => _setup();

    /// <summary>
    /// Queues the step and, where it is the first, installs the sequence on the setup: the call
    /// reports its arguments as it arrives and answers with whatever the queue says next.
    /// </summary>
    private void AppendStep(Func<IReadOnlyList<object>, TReturns?> answer)
    {
        var tap = _stepTap;
        var opens = _sequence!.IsEmpty;
        _sequence.Append(args =>
        {
            tap?.Invoke(args);
            return answer(args);
        });
        if (opens)
            OpenSequence();
    }

    /// <summary>
    /// Points the setup at the queue, in whichever way its return type is answered. A call that
    /// answers with nothing has no Returns to ask, so there the queue is driven by the callback
    /// that reports the invocation — the step still runs, it just has nothing to hand back.
    /// </summary>
    private void OpenSequence()
    {
        var sequence = _sequence!;
        Capturing(sequence.Capture);
        switch (Continuation)
        {
            case Moq.Language.Flow.IReturnsThrows<TService, TReturns?> sync:
                sync.Returns(sequence.Next);
                break;
            case Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns?>> async:
                async.ReturnsAsync(sequence.Next);
                break;
            case Moq.Language.Flow.IReturnsThrows<TService, ValueTask<TReturns?>> asyncValue:
                asyncValue.ReturnsAsync(sequence.Next);
                break;
            case Moq.Language.Flow.IReturnsThrows<TService, Task> task:
                task.Returns(() => { sequence.Next(); return Task.CompletedTask; });
                break;
            case Moq.Language.Flow.IReturnsThrows<TService, ValueTask> valueTask:
                valueTask.Returns(() => { sequence.Next(); return default(ValueTask); });
                break;
            case Moq.Language.ICallback plain:
                plain.Callback(new InvocationAction(_ => sequence.Next()));
                break;
            default:
                throw UnsupportedContinuation("First");
        }
    }

    /// <summary>
    /// Reports every invocation of the setup to the given action, arguments and all. One
    /// registration covers any signature, which is what lets a step of any arity read its call.
    /// </summary>
    private object Capturing(Action<IReadOnlyList<object>> observe)
    {
        var action = new InvocationAction(invocation => observe(invocation.Arguments));
        return Continuation switch
        {
            Moq.Language.ICallback<TService, TReturns?> sync => sync.Callback(action),
            Moq.Language.ICallback<TService, Task<TReturns?>> async => async.Callback(action),
            Moq.Language.ICallback<TService, ValueTask<TReturns?>> asyncValue => asyncValue.Callback(action),
            Moq.Language.ICallback<TService, Task> asyncVoid => asyncVoid.Callback(action),
            Moq.Language.ICallback<TService, ValueTask> asyncValueVoid => asyncValueVoid.Callback(action),
            Moq.Language.ICallback plain => plain.Callback(action),
            _ => throw UnsupportedContinuation("Tap"),
        };
    }

    private void SetupReturns()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockReturns();
        if (_sequence is not null)
            AppendStep(_ => Nothing());
        else
            InstallReturnsNothing();
    }

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
        if (_sequence is not null)
            AppendStep(_ => returns());
        else
            InstallReturns(returns);
    }

    private void SetupThrows<TException>()
        where TException : Exception, new()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows<TException>();
        if (_sequence is not null)
            AppendStep(_ => throw new TException());
        else
            InstallThrows<TException>();
    }

    private void SetupThrows(Func<Exception> expected, string expectedExpr)
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows(expectedExpr);
        if (_sequence is not null)
            AppendStep(_ => throw expected());
        else
            InstallThrows(expected);
    }

    private void InstallReturnsNothing()
    {
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task> taskContinuation)
            taskContinuation.Returns(Task.CompletedTask);
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, ValueTask> valueTaskContinuation)
            valueTaskContinuation.Returns(default(ValueTask));
        else if (Continuation is Moq.Language.Flow.ISetup<TService> voidContinuation)
            voidContinuation.Verifiable();
        else throw UnsupportedContinuation("Returns");
    }

    private void InstallReturns(Func<TReturns?> returns)
    {
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns?> syncContinuation)
            syncContinuation.Returns(returns);
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns?>> asyncContinuation)
            asyncContinuation.ReturnsAsync(returns);
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, ValueTask<TReturns?>> asyncValueContinuation)
            asyncValueContinuation.ReturnsAsync(returns);
        else throw UnsupportedContinuation("Returns");
    }

    private void InstallThrows<TException>()
        where TException : Exception, new()
    {
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns> syncContinuation)
            syncContinuation.Throws<TException>();
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns>> asyncContinuation)
            asyncContinuation.ThrowsAsync(It.IsAny<TException>());
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, ValueTask<TReturns>> asyncValueContinuation)
            asyncValueContinuation.ThrowsAsync(new TException());
        else if (Continuation is Moq.Language.Flow.ISetup<TService> setupContinuation)
            setupContinuation.Throws<TException>();
        else throw UnsupportedContinuation("Throws");
    }

    private void InstallThrows(Func<Exception> expected)
    {
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns> returnsThrows)
            returnsThrows.Throws(expected());
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns>> asyncReturnsThrows)
            asyncReturnsThrows.ThrowsAsync(expected());
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, ValueTask<TReturns>> asyncValueReturnsThrows)
            asyncValueReturnsThrows.ThrowsAsync(expected());
        else if (Continuation is Moq.Language.Flow.ISetup<TService> setupContinuation)
            setupContinuation.Throws(expected());
        else throw UnsupportedContinuation("Throws");
    }

    private SetupFailed UnsupportedContinuation(string setup)
        => new($"Cannot apply {setup} to '{_callExpr}': unhandled mock continuation {Continuation.GetType().Name}");

    private void SpecifyMock()
    {
        if (_callExpr is not null)
            _spec.Pipeline.Specification.AddMockSetup<TService>(_callExpr);
        foreach (var tapExpr in _tapExprs)
            _spec.Pipeline.Specification.AddTap(tapExpr);
    }
}
