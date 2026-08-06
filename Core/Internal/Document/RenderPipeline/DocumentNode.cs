using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

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
    internal static int CountArrangements(IReadOnlyList<SpecificationClause> clauses)
        => clauses.Count(clause => clause.Phase != StepPhase.Assert);

    internal virtual int ComplexityNumber => CountArrangements(Shared) + Children.Sum(child => child.ComplexityNumber);
}

/// The one node that holds requirements rather than nodes, and so ends the walk.
internal sealed record BranchNode(
    string Key, string? Heading, int Level, IReadOnlyList<SpecificationClause> Shared,
    Requirement[] Requirements)
    : DocumentNode(Key, Heading, Level, Shared, Declaration: default, Children: [])
{
    internal override int ComplexityNumber
        => CountArrangements(Shared) + Requirements.Sum(requirement => requirement.ArrangementCount);
}
