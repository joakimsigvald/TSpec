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
    private readonly List<List<SpecificationStep>> _clauses = new(10);
    private readonly List<SpecificationStep> _pending = [];
    private IReadOnlyList<SpecificationClause>? _cachedClauses;
    private bool _isIntroduced;
    private int _suppressionCount;
    private bool _isDescribed;
    private string? _because;
    private string? _cachedSpecification;

    /// <summary>
    /// The described clauses, in the order they were recorded — the hand-off to
    /// whatever renders them. Materialized on first use and safe to ask for
    /// repeatedly.
    /// </summary>
    internal IReadOnlyList<SpecificationClause> Clauses
    {
        get
        {
            Describe();
            return _cachedClauses ??= [.. _clauses.Select(clause => new SpecificationClause(clause))];
        }
    }

    /// The reason given for the requirement, rendered after the last step.
    internal string? Because => _because;

    public override string ToString()
        => _cachedSpecification ??= SpecificationRenderer
            .Compose(Clauses, _because)
            .Render(TextBuilder.PageWidth);

    /// Files a step under the statement it belongs to: anything but a word heads one of its own.
    internal void Add(SpecificationStep step)
    {
        if (step.Layout == StepLayout.Silent)
        {
            _pending.Add(step);
            return;
        }
        var startsStatement = step.Layout != StepLayout.Word;
        // Two introductions with nothing said between them are one statement
        if (startsStatement && step.Introduces && _isIntroduced)
            return;

        Place(step, startsStatement);
    }

    /// Records what an assertion claims: it fills the introduction in hand, or opens a statement.
    internal void Claim(SpecificationStep step) => Place(step, startsStatement: !_isIntroduced);

    private void Place(SpecificationStep step, bool startsStatement)
    {
        if (startsStatement || _clauses.Count == 0)
            _clauses.Add([]);
        _clauses[^1].AddRange(_pending);
        _clauses[^1].Add(step);
        _pending.Clear();
        _isIntroduced = step.Introduces;
    }

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

        // A silent step recorded last has no statement to wait for, so it joins the one before it.
        if (_pending.Count > 0 && _clauses.Count > 0)
            _clauses[^1].AddRange(_pending);
        _pending.Clear();

        // Reading Result inside a verification expression introduces a statement too late to say
        // anything. A bare Then is kept: there it is the whole assertion.
        if (_isIntroduced && _clauses.Count > 1 && SaysNothing(_clauses[^1]) && IsClaim(_clauses[^2]))
            _clauses.RemoveAt(_clauses.Count - 1);
    }

    private static bool SaysNothing(List<SpecificationStep> clause)
        => clause.All(step => step.Body.Length == 0);

    private static bool IsClaim(List<SpecificationStep> clause)
        => clause.Any(step => step.Layout != StepLayout.Silent
            && step.Family is StepFamily.None or StepFamily.Then);
}
