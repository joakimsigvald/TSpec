using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// One requirement of the document: the entry it came from, its steps split into clauses, and a
/// key for what it says. Clauses leave it as the headings above take them over.
/// </summary>
internal sealed record Requirement(
    SpecificationEntry Entry, IReadOnlyList<SpecificationClause> Clauses, string Signature)
{
    /// How much this requirement arranges before it claims anything.
    internal int Arrangement => DocumentNode.Arrangement(Clauses);

    /// The requirements of a run, one per distinct thing said.
    internal static IEnumerable<Requirement> From(IEnumerable<SpecificationEntry> entries)
        => entries
            .Select(entry => new Requirement(
                entry, SpecificationClause.Split(entry.Steps), ToSignature(entry)))
            .DistinctBy(requirement => (
                requirement.Entry.Namespace,
                requirement.Entry.Subject,
                requirement.Entry.Branch,
                requirement.Entry.Requirement,
                requirement.Signature));

    /// <summary>
    /// What a requirement says, as a key: the steps' own words and the reason given, with nothing
    /// of how they will be laid out. Two runs of one theory collapse to a single requirement, and
    /// two that differ in what they state never do — neither answer depending on the page width.
    /// </summary>
    private static string ToSignature(SpecificationEntry entry)
        => string.Join('\n', entry.Steps
            .Select(step => $"{step.Family}:{step.Body}")
            .Append(entry.Because ?? string.Empty));

    /// <summary>
    /// The clauses every one of these requirements states — written once under the heading they
    /// share instead of repeated in each block. Whole clauses only, never a fragment of one, and no
    /// minimum number of requirements: a lone one still belongs under the heading naming its context.
    /// </summary>
    /// <remarks>
    /// Assertions are excluded on principle: they are the claim each requirement exists to make, so
    /// two requirements agreeing on one is a coincidence to leave visible, not repetition to factor
    /// out. Arrangement and action are context, and context is what a heading is for.
    /// <para>
    /// The act rises only as far as the subject, whose heading is named after it — <paramref
    /// name="acts"/> is false above that, where nothing names it and it would stand over
    /// requirements whose own heading says they act otherwise.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<SpecificationClause> Shared(
        IReadOnlyList<Requirement> requirements, bool acts = true)
    {
        if (requirements.Count == 0)
            return [];

        // Hoisting must never empty a block, so the shortest requirement always keeps one clause.
        var limit = requirements.Min(requirement => requirement.Clauses.Count) - 1;
        List<SpecificationClause> shared = [];
        foreach (var clause in requirements[0].Clauses)
        {
            if (shared.Count == limit)
                break;
            if (!acts && clause.Family == StepFamily.When)
                continue;
            var taken = shared.Count(hoisted => hoisted.Matches(clause));
            if (clause.Phase != StepPhase.Assert
                && requirements.All(requirement => requirement.Clauses.Count(clause.Matches) > taken))
                shared.Add(clause);
        }
        return shared;
    }

    internal Requirement Without(IReadOnlyList<SpecificationClause> hoisted)
    {
        if (hoisted.Count == 0)
            return this;

        List<SpecificationClause> remaining = [.. Clauses];
        foreach (var clause in hoisted)
            remaining.Remove(remaining.FirstOrDefault(clause.Matches)!);
        return this with { Clauses = remaining };
    }
}
