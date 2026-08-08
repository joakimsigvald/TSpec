using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

internal sealed record Requirement(
    SpecificationEntry Entry, IReadOnlyList<SpecificationClause> Clauses, string Signature)
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

    internal static IEnumerable<Requirement> From(IEnumerable<SpecificationEntry> entries)
        => entries
            .Select(entry => new Requirement(entry, entry.Clauses, ToSignature(entry)))
            .DistinctBy(requirement => (
                requirement.Entry.Namespace,
                requirement.Entry.Subject,
                requirement.Entry.Branch,
                requirement.Entry.Requirement,
                requirement.Signature));

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
