using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// A node of the document: what heads it and at which level, what that heading declares, what it
/// states once for everything below, and what runs below. A node that heads nothing has no
/// <see cref="Heading"/> — and declares nothing either, there being no heading for that to be
/// declared at. Children are held in no particular order; ordering them is the renderer's.
/// </summary>
internal record DocumentNode(
    string Key, string? Heading, int Level, IReadOnlyList<SpecificationClause> Shared,
    Declared Declaration, IReadOnlyList<DocumentNode> Children)
{
    internal bool HasKey => !string.IsNullOrEmpty(Key);

    /// How much a set of clauses arranges, which is every phase but the claim itself.
    internal static int Arrangement(IReadOnlyList<SpecificationClause> clauses)
        => clauses.Count(clause => clause.Phase != StepPhase.Assert);

    /// <summary>
    /// The arrangement stated at this heading plus that of everything under it.
    /// </summary>
    /// <remarks>
    /// Assertions are not counted, and that is what makes summing upward safe: adding a requirement
    /// to an existing branch changes no number anywhere in the tree, so no section moves. Only
    /// arrangement appearing or disappearing can reorder the document, which is a structural change
    /// worth seeing in the diff. Counting size instead would reshuffle whole subjects because one
    /// line was added, spending the very thing the document is reviewed for.
    /// </remarks>
    internal virtual int ComplexityNumber
        => Arrangement(Shared) + Children.Sum(child => child.ComplexityNumber);
}

/// The one node that holds requirements rather than nodes, and so ends the walk.
internal sealed record BranchNode(
    string Key, string? Heading, int Level, IReadOnlyList<SpecificationClause> Shared,
    Requirement[] Requirements)
    : DocumentNode(Key, Heading, Level, Shared, Declaration: default, Children: [])
{
    internal override int ComplexityNumber
        => Arrangement(Shared) + Requirements.Sum(requirement => requirement.Arrangement);
}
