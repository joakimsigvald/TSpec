namespace TSpec.Internal.Specification;

/// <summary>
/// Records the specification of each pipeline step as a deferred description,
/// then renders the accumulated steps on <see cref="ToString"/>. Description
/// stays deferred because some steps only know what they say once the test has
/// run. Recording is suppressed while an assertion runs, so TSpec calls made
/// from inside asserts don't pollute the specification.
/// </summary>
internal class SpecificationRecording
{
    private readonly List<Action> _recordings = new(10);
    private readonly List<SpecificationStep> _steps = new(10);
    private int _suppressionCount;
    private bool _isDescribed;
    private string? _because;
    private string? _cachedSpecification;

    /// <summary>
    /// The described steps, in the order they were recorded — the hand-off to
    /// whatever renders them. Materialized on first use and safe to ask for
    /// repeatedly.
    /// </summary>
    internal IReadOnlyList<SpecificationStep> Steps
    {
        get
        {
            Describe();
            return _steps;
        }
    }

    /// The reason given for the requirement, rendered after the last step.
    internal string? Because => _because;

    public override string ToString()
        => _cachedSpecification ??= SpecificationRenderer.Render(Steps, _because, new TextBuilder());

    internal void Add(SpecificationStep step) => _steps.Add(step);

    internal void Record(Action describe)
    {
        if (_suppressionCount == 0)
            _recordings.Add(describe);
    }

    internal void SetBecause(string reason)
    {
        if (_suppressionCount > 0)
            return;

        if (_because is not null)
            throw new SetupFailed("Because can only be provided once per test method");

        _because = reason;
    }

    internal void SuppressRecording() => _suppressionCount++;

    internal void InciteRecording() => _suppressionCount--;

    /// <summary>
    /// The specification is frozen the first time it is observed. It has to be:
    /// <c>Specification.Is(...)</c> reads it from inside the test and then asserts, and that
    /// assertion is itself a recordable step — so without a freeze a test would describe the act
    /// of checking its own description.
    /// </summary>
    private void Describe()
    {
        if (_isDescribed)
            return;

        _isDescribed = true;
        foreach (var describe in _recordings)
            describe();
        _recordings.Clear();
    }
}
