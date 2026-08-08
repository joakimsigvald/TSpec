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
    /// The clauses every requirement states, to be said once above them. The act and what is
    /// claimed about it share a ceiling — the heading that names the method — since nothing above
    /// that heading says what the claim is a claim about. Only arrangement rises higher.
    /// </summary>
    internal static IReadOnlyList<SpecificationClause> Shared(
        IReadOnlyList<Requirement> requirements, bool actAndClaims = true)
    {
        if (requirements.Count == 0)
            return [];

        var limit = requirements.Min(requirement => requirement.Clauses.Count) - 1;
        List<SpecificationClause> shared = [];
        foreach (var clause in requirements[0].Clauses)
        {
            if (shared.Count == limit)
                break;
            if (!actAndClaims && (clause.Family == StepFamily.When || clause.Phase == StepPhase.Assert))
                continue;
            // A lone requirement claims for nobody but itself.
            if (clause.Phase == StepPhase.Assert && requirements.Count < 2)
                continue;
            var taken = shared.Count(hoisted => hoisted.Matches(clause));
            if (requirements.All(requirement => requirement.Clauses.Count(clause.Matches) > taken))
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
