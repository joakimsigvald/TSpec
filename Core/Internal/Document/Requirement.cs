using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

internal sealed record Requirement(
    SpecificationEntry Entry,
    IReadOnlyList<SpecificationClause> Clauses,
    string Signature,
    IReadOnlyList<TheoryRow> Rows)
{
    internal int ArrangementCount => DocumentNode.CountArrangements(Clauses);

    internal string Name
    {
        get
        {
            var name = Entry.Requirement.AsHeading();
            var stated = DocumentRenderer.StatedWord(name);
            return stated is not null
                && name.StartsWith($"{stated} ", StringComparison.Ordinal)
                    ? name[(stated.Length + 1)..]
                    : name;
        }
    }

    internal ComposedText Claim
        => SpecificationRenderer.Compose(Clauses, Entry.Because).Without(StepFamily.Then.Keyword());

    internal int Size
        => Clauses.Sum(clause => clause.Steps.Sum(step => step.Body.Length))
            + (Entry.Because?.Length ?? 0);

    /// <summary>
    /// Where a theory filled a hole, the document leaves it open: the value is the row's, not the
    /// requirement's, and the table beneath states every row's. That is also what makes the rows
    /// of one theory the same requirement, since they then describe themselves identically.
    /// </summary>
    /// <remarks>
    /// What reported identically is one requirement — a theory's rows, and a requirement written
    /// above the branches it ran in. The rows are collected rather than discarded, which is the
    /// difference between a theory stating its data and stating only its parameter names.
    /// </remarks>
    internal static IEnumerable<Requirement> From(IEnumerable<SpecificationEntry> entries)
        => entries
            .Select(entry => entry with { Clauses = Hole.Hollow(entry.Clauses) })
            .GroupBy(entry => (
                entry.Namespace,
                entry.Subject,
                entry.Branch,
                entry.Requirement,
                Signature: ToSignature(entry)))
            .Select(reported => new Requirement(
                reported.First(), reported.First().Clauses, reported.Key.Signature,
                RowsOf(reported)));

    /// Declaration order, since rows report in whatever order a parallel run finished them.
    private static IReadOnlyList<TheoryRow> RowsOf(IEnumerable<SpecificationEntry> reported)
        => [.. reported.Select(entry => entry.Row).OfType<TheoryRow>().OrderBy(row => row.Index)];

    private static string ToSignature(SpecificationEntry entry)
        => string.Join('\n', entry.Clauses
            .SelectMany(clause => clause.Steps)
            .Select(step => $"{step.Family}:{step.Body}")
            .Append(entry.Because ?? string.Empty));

    internal static string? SubjectOf(IReadOnlyList<Requirement> requirements)
    {
        var first = requirements.FirstOrDefault()?.Entry;
        return first?.SubjectUnderTest is not null
            && requirements.All(r => r.Entry.SubjectUnderTest == first.SubjectUnderTest)
                ? first.SubjectUnderTest
                : null;
    }

    internal static string? ReturnTypeOf(IReadOnlyList<Requirement> requirements)
    {
        var first = requirements.FirstOrDefault()?.Entry;
        return first?.SubjectUnderTest is not null
            && requirements.All(r => r.Entry.ReturnType == first.ReturnType)
                ? first.ReturnType
                : null;
    }

    /// <summary>
    /// The arrangement every requirement states, to be said once above them. Assertions never rise
    /// on their own: a requirement is the smallest thing that may be hoisted, so one technical
    /// assertion made by two requirements stays visible in both. The act rises no higher than the
    /// heading that names the method.
    /// </summary>
    internal static IReadOnlyList<SpecificationClause> Shared(
        IReadOnlyList<Requirement> requirements, bool acts = true)
    {
        if (requirements.Count == 0)
            return [];

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

    /// <summary>
    /// Two requirements a reader cannot tell apart: the same name over the same statements, which is
    /// what one requirement written above its branches looks like once it has run in each of them.
    /// </summary>
    internal bool Restates(Requirement other)
        => Entry.Requirement == other.Entry.Requirement
            && Entry.Because == other.Entry.Because
            && Clauses.Count == other.Clauses.Count
            && Clauses.Zip(other.Clauses).All(pair => pair.First.Matches(pair.Second));

    /// <summary>
    /// The requirements every branch repeats, to be listed once above them. One branch has nobody to
    /// repeat anything with, and the take leaves every branch an item of its own.
    /// </summary>
    internal static IReadOnlyList<Requirement> Repeated(IReadOnlyList<DocumentNode> branches)
        => branches.Count < 2
            ? []
            : [.. branches[0].Requirements
                .Where(candidate => branches.All(
                    branch => branch.Requirements.Any(candidate.Restates)))
                .Take(branches.Min(branch => branch.Requirements.Count) - 1)];

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
