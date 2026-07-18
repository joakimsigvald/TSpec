namespace TSpec.Internal.Specification;

/// <summary>
/// Records the specification phrase of each pipeline step as a deferred
/// rendering action, and renders the accumulated text on <see cref="ToString"/>.
/// Recording is suppressed while an assertion runs, so TSpec calls made from
/// inside asserts don't pollute the specification.
/// </summary>
internal class SpecificationRecording(TextBuilder textBuilder)
{
    private readonly List<Action> _recordings = new(10);
    private int _suppressionCount;
    private string? _because;
    private string? _cachedSpecification;

    public override string ToString()
    {
        if (_cachedSpecification is not null)
            return _cachedSpecification;

        foreach (var render in _recordings)
            render();

        if (_because is not null)
            textBuilder.AddWord($"because {_because}", ", ");

        return _cachedSpecification = textBuilder.ToString();
    }

    internal void Record(Action render)
    {
        if (_suppressionCount == 0)
            _recordings.Add(render);
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
}
