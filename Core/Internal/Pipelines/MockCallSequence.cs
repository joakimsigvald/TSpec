namespace TSpec.Internal.Pipelines;

/// <summary>
/// The answers a mocked call gives, in order.
/// </summary>
/// <remarks>
/// The sequence sits behind the call's one answer, where a step can tap the very call it answers —
/// and where the outcome of a step is described in exactly the terms a single setup already uses.
/// A tap written before the sequence opened taps every call of it, as a tap outside one does.
/// </remarks>
internal sealed class MockCallSequence<TReturns>(Action<IReadOnlyList<object>>? tap, IReadOnlyList<string> tapExprs)
{
    private readonly List<Func<IReadOnlyList<object>, TReturns?>> _steps = [];
    private int _next;

    /// Whether nothing has been queued yet, which is what makes a step the one that installs it.
    internal bool IsEmpty => _steps.Count == 0;

    /// The taps written before the sequence opened, stated before it.
    internal IReadOnlyList<string> TapExprs => tapExprs;

    internal void Append(Func<IReadOnlyList<object>, TReturns?> step) => _steps.Add(step);

    /// Taps the call, then answers it with the next step; past the last step there is none to answer with.
    internal bool TryNext(IReadOnlyList<object> arguments, out TReturns? answer)
    {
        tap?.Invoke(arguments);
        if (_next == _steps.Count)
        {
            answer = default;
            return false;
        }

        answer = _steps[_next++](arguments);
        return true;
    }
}
