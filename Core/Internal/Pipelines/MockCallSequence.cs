namespace TSpec.Internal.Pipelines;

/// <summary>
/// The answers a mocked call gives, in order, and the arguments of the call being answered.
/// </summary>
/// <remarks>
/// Moq's own SetupSequence would keep the order, but what it hands back is not a setup: it has no
/// Callback, so a sequence could state what each call answers or what each call was asked, never
/// both. Owning the sequence puts it back on one ordinary setup, where a step can tap the very call
/// it answers — and where the outcome of a step is described in exactly the terms a single setup
/// already uses.
/// </remarks>
internal sealed class MockCallSequence<TReturns>
{
    private readonly List<Func<IReadOnlyList<object>, TReturns?>> _steps = [];
    private IReadOnlyList<object> _arguments = [];
    private int _next;

    /// Whether nothing has been queued yet, which is what makes a step the one that installs it.
    internal bool IsEmpty => _steps.Count == 0;

    /// Recorded as the call arrives, so the step answering it sees the arguments it was called with.
    internal void Capture(IReadOnlyList<object> arguments) => _arguments = arguments;

    internal void Append(Func<IReadOnlyList<object>, TReturns?> step) => _steps.Add(step);

    /// Past the end a sequence answers with the type's default, which is what Moq's own does.
    internal TReturns? Next() => _next < _steps.Count ? _steps[_next++](_arguments) : default;
}
