namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// What a heading declares about the code it describes. Each label is carried separately, and null
/// where the requirements below disagree and the statement belongs further down instead — a subject
/// can hold for a whole document while every section returns something different.
/// </summary>
internal readonly record struct Declared(string? Subject, string? ReturnType)
{
    private const string SubjectLabel = "Subject under test:";
    private const string ReturnLabel = "Return type:";

    /// <summary>
    /// What these requirements agree on, and so what the heading above them may declare.
    /// </summary>
    /// <remarks>
    /// The return type rises only as far as the subject, for the reason the act does — the heading
    /// there is named after the method, and above it nothing is. The subject names a class rather
    /// than a call, so it carries no such tie and rises as far as it holds.
    /// </remarks>
    internal static Declared Of(IReadOnlyList<Requirement> requirements, bool returns = true)
    {
        var first = requirements.FirstOrDefault()?.Entry;
        if (first?.SubjectUnderTest is null)
            return default;

        return new(
            requirements.All(r => r.Entry.SubjectUnderTest == first.SubjectUnderTest)
                ? first.SubjectUnderTest : null,
            returns && requirements.All(r => r.Entry.ReturnType == first.ReturnType)
                ? first.ReturnType : null);
    }

    /// What is left to declare here, given what the headings above already have.
    internal Declared Except(Declared stated)
        => new(stated.Subject is null ? Subject : null,
            stated.ReturnType is null ? ReturnType : null);

    /// Everything declared so far, so that one <see cref="Except"/> covers every heading above.
    internal Declared And(Declared below)
        => new(Subject ?? below.Subject, ReturnType ?? below.ReturnType);

    /// <summary>
    /// One space after each label, never a column: the two hoist independently, so aligning them
    /// would make where a value starts depend on which other label happens to be stated beside it.
    /// </summary>
    internal string? Text => (Subject, ReturnType) switch
    {
        (null, null) => null,
        (not null, null) => $"{SubjectLabel} {Subject}",
        (null, not null) => ReturnLine,
        _ => $"{SubjectLabel} {Subject}\n{ReturnLine}",
    };

    /// The return type as a label of its own, for where it follows the clauses instead.
    internal string? ReturnLine => ReturnType is null ? null : $"{ReturnLabel} {ReturnType}";
}
