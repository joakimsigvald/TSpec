using System.Runtime.CompilerServices;

namespace TSpec.Internal.Pipelines;

internal class SpecFixture<TSUT>(ISpecificationProvider specificationProvider) : IDisposable
{
    private bool _disposed;
    private Lazy<TSUT>? _sut;
    private readonly List<Command> _setUp = [];
    private readonly List<Command> _tearDown = [];

    internal void After(Command setUp) => _setUp.Insert(0, setUp);
    internal void Before(Command tearDown) => _tearDown.Add(tearDown);
    internal void SetUp(Lazy<TSUT> sut)
    {
        _sut = sut;
        foreach (var setUp in _setUp)
            Invoke<object>(setUp);
        IsSetUp = true;
    }

    internal bool IsSetUp { get; private set; } = false;
    internal TSUT SubjectUnderTest => _sut!.Value;

    internal void TearDown()
    {
        foreach (var tearDown in _tearDown)
            Invoke<object>(tearDown);
    }

    internal void AddToSpecification()
    {
        foreach (var setUp in _setUp.Reverse<Command>())
            specificationProvider.Specification.AddAfter(setUp.Expression);
        foreach (var tearDown in _tearDown)
            specificationProvider.Specification.AddBefore(tearDown.Expression);
        specificationProvider.Specification.AddThen();
    }

    internal (TResult result, bool hasResult) Invoke<TResult>(
        Command command, [CallerArgumentExpression(nameof(command))] string? commandName = null)
    {
        return command.Invocation switch
        {
            Func<TSUT, Task<TResult>> queryAsync => (AsyncHelper.Execute(() => queryAsync(SubjectUnderTest)), true),
            Func<Task<TResult>> queryAsync => (AsyncHelper.Execute(() => queryAsync()), true),
            Func<TSUT, Task> actAsync => ActOnSubjectAndReturnAsync(actAsync),
            Func<Task> actAsync => ActAndReturnAsync(actAsync),
            Func<TSUT, TResult> query => (query(SubjectUnderTest), true),
            Func<TResult> query => (query(), true),
            Action<TSUT> act => ActOnSubjectAndReturn(act),
            Action act => ActAndReturn(act),
            _ => throw new SetupFailed($"Failed to run {commandName!}, unexpected signature")
        };

        (TResult, bool) ActAndReturn(Action act)
        {
            act();
            return (default!, false);
        }

        (TResult, bool) ActOnSubjectAndReturn(Action<TSUT> act)
        {
            act(SubjectUnderTest);
            return (default!, false);
        }

        (TResult, bool) ActAndReturnAsync(Func<Task> actAsync)
        {
            AsyncHelper.Execute(() => actAsync());
            return (default!, false);
        }

        (TResult, bool) ActOnSubjectAndReturnAsync(Func<TSUT, Task> actAsync)
        {
            AsyncHelper.Execute(() => actAsync(SubjectUnderTest));
            return (default!, false);
        }
    }

    /// <summary>
    /// Runs the teardown of the fixture (the steps provided with Before), at most once
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        TearDown();
    }
}