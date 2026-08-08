using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record CodeSegment(
    string? SubjectUnderTest,
    string? ReturnType,
    IReadOnlyList<SpecificationClause> Shared,
    string? Stated) : DocumentSegment
{
    private const string SubjectLabel = "Subject under test:";
    private const string ReturnLabel = "Return type:";

    internal override string Render()
    {
        var body = Body();
        return body.Length == 0 ? string.Empty : Block(body);
    }

    private string Body()
    {
        var acts = Shared.Any(clause => clause.Family == StepFamily.When);
        var joins = ReturnType is not null && acts && Joins();
        // Said under the act where it did not fit on it: it qualifies the act, so it follows it
        // rather than heading a block it is not part of. With no act to follow it has only the
        // subject to stand beside, and stays where that is.
        var trails = !joins && ReturnType is not null && acts;
        var labelled = joins || trails ? null : ReturnType;
        string?[] parts = [
            Labels(labelled),
            Shared.Count == 0
                ? null
                : Clauses(joins ? ReturnType : null, SubjectUnderTest is not null || labelled is not null),
            trails ? Returns(ReturnType!) : null];
        return string.Join("\n", parts.Where(part => !string.IsNullOrEmpty(part)));
    }

    /// <summary>
    /// One space after each label, never a column: the two hoist independently, so aligning them
    /// would make where a value starts depend on which other label happens to be stated beside it.
    /// </summary>
    private string? Labels(string? returnType) => (SubjectUnderTest, returnType) switch
    {
        (null, null) => null,
        (not null, null) => $"{SubjectLabel} {SubjectUnderTest}",
        (null, not null) => Returns(returnType),
        _ => $"{SubjectLabel} {SubjectUnderTest}\n{Returns(returnType)}",
    };

    private static string Returns(string returnType) => $"{ReturnLabel} {returnType}";

    /// <summary>
    /// Whether the return type may be said on the act, which it may only where it costs no line: a
    /// trailing phrase that wraps starts its line with the binder's comma, breaking a statement
    /// where nothing relates the halves. Where it does not fit it stays the label it already is at
    /// a heading with no act — said once either way, only in the other of its two forms.
    /// </summary>
    private bool Joins()
        => Lines(Clauses(ReturnType, SubjectUnderTest is not null))
            == Lines(Clauses(null, SubjectUnderTest is not null));

    /// A label above the clauses opens the block, so the clauses keep the heading's word.
    private string Clauses(string? returns, bool labelled)
        => SpecificationRenderer.Compose(Shared, because: null, returns)
            .Without(labelled ? null : Stated)
            .Fit(Document.Width);

    private static string Block(string specification)
        => specification.Contains('\n')
            ? $"```\n{specification}\n```\n"
            : $"`{specification}`\n";

    private static int Lines(string text) => text.Count(character => character == '\n');
}
