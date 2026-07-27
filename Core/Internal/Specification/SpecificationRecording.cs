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

    private void Describe()
    {
        foreach (var describe in _recordings)
            describe();
        _recordings.Clear();
    }
}
