using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record CodeSegment(
    Declared Declared, IReadOnlyList<SpecificationClause> Shared, string? Stated) : DocumentSegment
{
    internal override string Render()
    {
        var body = Body();
        return body.Length == 0 ? string.Empty : Block(body);
    }

    private string Body()
    {
        var above = Declared with { ReturnType = null };
        var joins = Declared.ReturnType is not null
            && Shared.Any(clause => clause.Family == StepFamily.When)
            && Joins(above, Declared.ReturnType);
        var trails = !joins && Declared.ReturnType is not null && Shared.Count > 0;
        var says = joins || trails ? above : Declared;
        var clauses = Shared.Count == 0
            ? null
            : Clauses(says, joins ? Declared.ReturnType : null);
        string?[] parts = [says.Text, clauses, trails ? Declared.ReturnLine : null];
        return string.Join("\n", parts.Where(part => !string.IsNullOrEmpty(part)));
    }


    private static string Block(string specification)
        => specification.Contains('\n')
            ? $"```\n{specification}\n```\n"
            : $"`{specification}`\n";

    /// <summary>
    /// Whether the return type may be said on the act, which it may only where it costs no line: a
    /// trailing phrase that wraps starts its line with the binder's comma, breaking a statement
    /// where nothing relates the halves. Where it does not fit it stays the label it already is at
    /// a heading with no act — said once either way, only in the other of its two forms.
    /// </summary>
    private bool Joins(Declared says, string returns)
        => Lines(Clauses(says, returns)) == Lines(Clauses(says, null));

    /// A label above the clauses opens the block, so the clauses keep the heading's word.
    private string Clauses(Declared says, string? returns)
        => SpecificationRenderer.Compose(Shared, because: null, returns).Without(says.Text is null ? Stated : null).Fit(Document.Width);

    private static int Lines(string text) => text.Count(character => character == '\n');
}
