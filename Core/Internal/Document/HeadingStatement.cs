using TSpec.Internal.Specification;
using static TSpec.Internal.Document.DocumentText;

namespace TSpec.Internal.Document;

/// <summary>
/// What a heading says about itself: what it declares, then the clauses hoisted to it. Nothing to
/// say means nothing is written at all, so a heading never opens an empty block.
/// </summary>
internal static class HeadingStatement
{
    /// <summary>
    /// What a heading states, unfenced: what it declares, then the clauses hoisted to it. Empty
    /// where there is nothing to say, so a heading never opens an empty block.
    /// </summary>
    internal static string Body(
        Declared declared, IReadOnlyList<SpecificationClause> shared, string? stated)
    {
        var above = declared with { ReturnType = null };
        var joins = declared.ReturnType is not null
            && shared.Any(clause => clause.Family == StepFamily.When)
            && Joins(above, shared, declared.ReturnType, stated);
        // Said under the clauses where it did not fit on the act: it qualifies what they say, so it
        // follows them rather than heading a block it is not part of. With no clauses to follow it
        // has only the subject to stand beside, and stays where that is.
        var trails = !joins && declared.ReturnType is not null && shared.Count > 0;
        var says = joins || trails ? above : declared;
        var clauses = shared.Count == 0
            ? null
            : Clauses(says, shared, joins ? declared.ReturnType : null, stated);
        string?[] parts = [says.Text, clauses, trails ? declared.ReturnLine : null];
        return string.Join("\n", parts.Where(part => !string.IsNullOrEmpty(part)));
    }

    /// <summary>
    /// Whether the return type may be said on the act, which it may only where it costs no line: a
    /// trailing phrase that wraps starts its line with the binder's comma, breaking a statement
    /// where nothing relates the halves. Where it does not fit it stays the label it already is at
    /// a heading with no act — said once either way, only in the other of its two forms.
    /// </summary>
    private static bool Joins(
        Declared says, IReadOnlyList<SpecificationClause> shared, string returns, string? stated)
        => Lines(Clauses(says, shared, returns, stated)) == Lines(Clauses(says, shared, null, stated));

    /// A label above the clauses opens the block, so the clauses keep the heading's word.
    private static string Clauses(
        Declared says, IReadOnlyList<SpecificationClause> shared, string? returns, string? stated)
        => Fit(
            Compose(shared, because: null, returns).Without(says.Text is null ? stated : null),
            DocumentWidth);
}
