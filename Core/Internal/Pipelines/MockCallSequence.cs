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

    /// Past the end a sequence answers with the type's default.
    internal TReturns? Next(IReadOnlyList<object> arguments)
    {
        tap?.Invoke(arguments);
        return _next < _steps.Count ? _steps[_next++](arguments) : default;
    }
}
