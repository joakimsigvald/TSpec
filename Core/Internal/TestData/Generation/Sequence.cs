namespace TSpec.Internal.TestData.Generation;

/// <summary>
/// A source value sequence configured with StartingAt and Spaced.
/// </summary>
internal interface ISequence
{
    object? Next();
    string Describe();
}

/// <summary>
/// Produces the source values of a type relay: from the start,
/// each subsequent value is computed by the step function.
/// The concrete sequence type provides the default start and step for its value type.
/// Producing a duplicate, or stepping outside the value type's range, throws ValuesExhausted.
/// Duplicate detection is delegated to a guard matched to the kind of step when the step is set.
/// </summary>
internal abstract class Sequence<TValue> : ISequence
{
    private (TValue value, string expr)? _start;
    private (Func<TValue, int, TValue> apply, string expr)? _step;
    private IDuplicateGuard _guard = new IntervalGuard();
    private int _count;
    private TValue _current = default!;

    private TValue Start => _start is { } start ? start.value : DefaultStart;
    private Func<TValue, int, TValue> Step => _step is { } step ? step.apply : (current, _) => DefaultStep(current);

    protected abstract TValue DefaultStart { get; }

    /// <summary>
    /// The default step of the concrete sequence type, applying a constant interval to the previous value
    /// (so the sequence can rely on the O(1) duplicate guard).
    /// </summary>
    protected abstract TValue DefaultStep(TValue current);

    internal void SetStart(TValue start, string startExpr)
    {
        if (_start is not null)
            throw new SetupFailed("StartingAt can only be applied once per From");
        _start = (start, startExpr);
    }

    /// <summary>
    /// Set a step that applies a constant interval to the previous value,
    /// letting the sequence keep the O(1) duplicate guard.
    /// </summary>
    internal void SetInterval(Func<TValue, TValue> next, string stepExpr)
        => ApplyStep((current, _) => next(current), stepExpr);

    /// <summary>
    /// Set a custom step function, which can produce any value,
    /// so the sequence guards against duplicates by collecting all values produced.
    /// </summary>
    internal void SetStep(Func<TValue, int, TValue> step, string stepExpr)
    {
        ApplyStep(step, stepExpr);
        _guard = new CollectAllGuard();
    }

    public object? Next()
    {
        var next = _count == 0 ? Start : Advance();
        if (_guard.IsDuplicate(next))
            throw new ValuesExhausted(typeof(TValue));
        _count++;
        _current = next;
        return next;
    }

    public string Describe()
        => (_start is { } start ? $" starting at {start.expr}" : string.Empty)
        + (_step is { } step ? $" spaced {step.expr}" : string.Empty);

    private void ApplyStep(Func<TValue, int, TValue> step, string stepExpr)
    {
        if (_step is not null)
            throw new SetupFailed("Spaced can only be applied once per From");
        _step = (step, stepExpr);
    }

    private TValue Advance()
    {
        try
        {
            return Step(_current, _count);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or OverflowException)
        {
            throw new ValuesExhausted(typeof(TValue));
        }
    }

    /// <summary>
    /// Detects duplicates among the values produced by a sequence.
    /// Checking a value also records it, so detection and bookkeeping cannot get out of step.
    /// </summary>
    private interface IDuplicateGuard
    {
        bool IsDuplicate(TValue value);
    }

    /// <summary>
    /// The O(1) duplicate guard for steps that apply a constant interval: such a step can only
    /// revisit the first value (on types that wrap around) or repeat the previous value
    /// (on types that stall at a precision or range limit).
    /// </summary>
    private sealed class IntervalGuard : IDuplicateGuard
    {
        private static readonly EqualityComparer<TValue> _comparer = EqualityComparer<TValue>.Default;
        private (TValue first, TValue previous)? _seen;

        public bool IsDuplicate(TValue value)
        {
            if (_seen is not { } seen)
            {
                _seen = (value, value);
                return false;
            }
            if (_comparer.Equals(value, seen.previous) || _comparer.Equals(value, seen.first))
                return true;
            _seen = (seen.first, value);
            return false;
        }
    }

    /// <summary>
    /// The duplicate guard for custom step functions, which can produce any value,
    /// so every value produced is collected.
    /// </summary>
    private sealed class CollectAllGuard : IDuplicateGuard
    {
        private readonly HashSet<TValue> _produced = [];

        public bool IsDuplicate(TValue value) => !_produced.Add(value);
    }
}