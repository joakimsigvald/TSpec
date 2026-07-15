using Moq;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Pipelines;

internal abstract class GivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    : IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    private readonly Spec<TSUT, TResult> _spec;
    private readonly Lazy<object> _lazyContinuation;
    private readonly Func<bool, object> _setup;
    private readonly string _callExpr;
    private readonly string? _tapExpr;
    internal readonly bool _isSequential;

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Func<Mock<TService>, bool, object> setup,
        string callExpr)
        : this(spec, isSequential => setup(spec.GetMock<TService>(), isSequential), callExpr)
    {
    }

    protected GivenThatCommonContinuation(
        Spec<TSUT, TResult> spec,
        Func<bool, object> setup,
        string callExpr,
        string? tapExpr = null,
        Lazy<object>? lazyContinuation = null,
        bool isSequential = false)
    {
        _spec = spec;
        _setup = setup;
        _callExpr = callExpr;
        _tapExpr = tapExpr;
        _lazyContinuation = lazyContinuation ?? new Lazy<object>(DoSetup);
        _isSequential = isSequential;
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns()
    {
        _spec.AppendGiven(SetupReturns);
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, this);
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Func<TReturns?> returns, [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null)
    {
        _spec.AppendGiven(() => SetupReturns(returns!, returnsExpr!));
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, this);
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Tag<TReturns?> tag, [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Returns(() => _spec.The(tag), tagExpr!);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> ReturnsDefault()
        => Returns(() => default);

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws<TException>()
        where TException : Exception, new()
    {
        _spec.AppendGiven(SetupThrows<TException>);
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, this);
    }

    public IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        _spec.AppendGiven(() => SetupThrows(expected, expectedExpr!));
        return new GivenThatReturnsContinuation<TSUT, TResult, TService, TReturns>(_spec, this);
    }

    public IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> First()
        => InSequence(_callExpr + " first");

    internal GivenThatNextContinuation<TSUT, TResult, TService, TReturns> AndNext()
        => InSequence("next", _lazyContinuation);

    protected GivenThatNextContinuation<TSUT, TResult, TService, TReturns> ContinueWith(
        Func<IFluentInterface> callback, string? tapExpr = null)
        => new(_spec, _ => callback(), _callExpr, tapExpr);

    protected object Continuation => _lazyContinuation.Value;

    private GivenThatNextContinuation<TSUT, TResult, TService, TReturns> InSequence(
        string callExpr, Lazy<object>? lazyContinuation = null)
        => new(_spec, _setup, callExpr, lazyContinuation: lazyContinuation, isSequential: true);

    private object DoSetup() => _setup(_isSequential);

    private void SetupReturns()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockReturns();
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task> taskContinuation)
            taskContinuation.Returns(Task.CompletedTask);
        else if (Continuation is Moq.Language.Flow.ISetup<TService> voidContinuation)
            voidContinuation.Verifiable();
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task> sequentialTaskContinuation)
            sequentialTaskContinuation.Returns(Task.CompletedTask);
        else if (Continuation is Moq.Language.ISetupSequentialResult<TReturns>)
            return;
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task<TReturns?>> sequentialAsyncContinuation)
            sequentialAsyncContinuation.Returns(Task.FromResult(default(TReturns)));
        else throw new NotImplementedException();
    }

    private void SetupReturns(Func<TReturns?> returns, string returnsExpr)
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockReturns(returnsExpr);
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns?> syncContinuation)
            syncContinuation.Returns(returns);
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns?>> asyncContinuation)
            asyncContinuation.ReturnsAsync(returns);
        else if (Continuation is Moq.Language.ISetupSequentialResult<TReturns?> sequentialContinuation)
            sequentialContinuation.Returns(returns);
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task<TReturns?>> sequentialAsyncContinuation)
            sequentialAsyncContinuation.ReturnsAsync(returns);
        else throw new NotImplementedException();
    }

    private void SetupThrows<TException>()
        where TException : Exception, new()
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows<TException>();
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns> syncContinuation)
            syncContinuation.Throws<TException>();
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns>> asyncContinuation)
            asyncContinuation.ThrowsAsync(It.IsAny<TException>());
        else if (Continuation is Moq.Language.ISetupSequentialResult<TReturns> sequentialContinuation)
            sequentialContinuation.Throws<TException>();
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task<TReturns>> sequentialAsyncContinuation)
            sequentialAsyncContinuation.ThrowsAsync(It.IsAny<TException>());
        else if (Continuation is Moq.Language.Flow.ISetup<TService> setupContinuation)
            setupContinuation.Throws<TException>();
        else throw new NotImplementedException();
    }

    private void SetupThrows(Func<Exception> expected, string expectedExpr)
    {
        SpecifyMock();
        _spec.Pipeline.Specification.AddMockThrows(expectedExpr);
        if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, TReturns> returnsThrows)
            returnsThrows.Throws(expected());
        else if (Continuation is Moq.Language.Flow.IReturnsThrows<TService, Task<TReturns>> asyncReturnsThrows)
            asyncReturnsThrows.ThrowsAsync(expected());
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task> sequentialTaskContinuation)
            sequentialTaskContinuation.ThrowsAsync(expected());
        else if (Continuation is Moq.Language.ISetupSequentialResult<TReturns> sequentialContinuation)
            sequentialContinuation.Throws(expected());
        else if (Continuation is Moq.Language.ISetupSequentialResult<Task<TReturns>> sequentialAsyncContinuation)
            sequentialAsyncContinuation.ThrowsAsync(expected());
        else if (Continuation is Moq.Language.Flow.ISetup<TService> setupContinuation)
            setupContinuation.Throws(expected());
        else throw new NotImplementedException();
    }

    private void SpecifyMock()
    {
        if (_callExpr is not null)
            _spec.Pipeline.Specification.AddMockSetup<TService>(_callExpr);
        if (_tapExpr is not null)
            _spec.Pipeline.Specification.AddTap(_tapExpr);
    }
}