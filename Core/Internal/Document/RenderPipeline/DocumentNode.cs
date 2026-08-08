using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// A node of the document: what heads it and at which level, what that heading declares, what it
/// states once for everything below, the requirements listed under it, and what runs below. A node
/// that heads nothing has no <see cref="Heading"/> — and declares nothing either, there being no
/// heading for that to be declared at. Children are held in no particular order; ordering them is
/// the renderer's.
/// </summary>
/// <remarks>
/// A node holds requirements and children alike: a branch is where requirements usually end up, but
/// one every branch repeats rises to the heading they share and is listed there, above them.
/// </remarks>
internal sealed record DocumentNode(
    string Key, string? Heading, int Level, IReadOnlyList<SpecificationClause> Shared,
    string? SubjectUnderTest, string? ReturnType, IReadOnlyList<DocumentNode> Children,
    IReadOnlyList<Requirement> Requirements)
{
    internal DocumentNode(
        string Key, string? Heading, int Level, IReadOnlyList<SpecificationClause> Shared,
        IReadOnlyList<Requirement> Requirements)
        : this(Key, Heading, Level, Shared,
            SubjectUnderTest: null, ReturnType: null, Children: [], Requirements)
    { }

    internal bool HasKey => !string.IsNullOrEmpty(Key);

    /// Drops what is now listed at the heading above.
    internal DocumentNode Without(IReadOnlyList<Requirement> lifted)
        => this with
        {
            Requirements = [.. Requirements.Where(mine => !lifted.Any(mine.Restates))],
        };

    /// How much a set of clauses arranges, which is every phase but the claim itself.
    internal static int CountArrangements(IReadOnlyList<SpecificationClause> clauses)
        => clauses.Count(clause => clause.Phase != StepPhase.Assert);

    internal int ComplexityNumber
        => CountArrangements(Shared)
            + Requirements.Sum(requirement => requirement.ArrangementCount)
            + Children.Sum(child => child.ComplexityNumber);
}
