using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record Document(
    SpecificationSubject Subject,
    string SpecAssemblyName,
    string BuildId,
    string? SubjectUnderTest,
    string? ReturnType,
    IReadOnlyList<SpecificationClause> Whole,
    IReadOnlyList<DocumentNode> Areas)
{
    internal const int Width = 90;

    internal static Document Of(
        SpecificationSubject subject, string specAssemblyName, string buildId,
        IEnumerable<SpecificationEntry> entries)
    {
        Requirement[] requirements = [.. Requirement.From(entries)];
        var whole = Requirement.Shared(requirements, acts: false);
        return new(subject, specAssemblyName, buildId,
            Requirement.SubjectOf(requirements), Requirement.ReturnTypeOf(requirements),
            whole, ToAreas(requirements, whole));
    }

    private const int AreaLevel = 1;

    private const int GroupLevel = AreaLevel + 1;

    /// Past four, a heading stops telling a reader where they are.
    private const int MaxLevel = 4;

    private static DocumentNode[] ToAreas(
        Requirement[] requirements, IReadOnlyList<SpecificationClause> hoisted)
    {
        var rootDepth = CommonRootDepth(requirements);
        return [.. requirements
            .Select(requirement => requirement.Without(hoisted))
            .GroupBy(requirement => AreaOf(requirement.Entry.Namespace, rootDepth))
            .Select(area => ToArea(area, rootDepth))];
    }

    private static DocumentNode ToArea(IGrouping<string, Requirement> area, int rootDepth)
    {
        var ofArea = area.ToArray();
        var heads = area.Key.Length > 0;
        var shared = heads ? Requirement.Shared(ofArea, acts: false) : [];
        var subject = heads ? Requirement.SubjectOf(ofArea) : null;
        var groups = ofArea
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => GroupOf(requirement.Entry.Namespace, rootDepth + 1))
            .ToArray();
        return new(area.Key, heads ? area.Key.AsHeading() : null, AreaLevel, shared,
            subject, heads ? Requirement.ReturnTypeOf(ofArea) : null,
            [.. groups.Select(group => ToGroup(group, heads: groups.Length > 1))],
            Requirements: []);
    }

    private static DocumentNode ToGroup(IGrouping<string, Requirement> group, bool heads)
    {
        var ofGroup = group.ToArray();
        var shared = heads ? Requirement.Shared(ofGroup, acts: false) : [];
        var subject = heads ? Requirement.SubjectOf(ofGroup) : null;
        var subjectLevel = heads ? GroupLevel + 1 : GroupLevel;
        return new(group.Key, heads ? group.Key.AsTitle() : null, GroupLevel, shared,
            subject, heads ? Requirement.ReturnTypeOf(ofGroup) : null,
            [.. ofGroup
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => requirement.Entry.Subject)
            .Select(subject => ToSubject(subject, subjectLevel))],
            Requirements: []);
    }

    /// <summary>
    /// The heading that names the act, and the last one a requirement may rise to: above it nothing
    /// says what the requirement is a requirement about.
    /// </summary>
    private static DocumentNode ToSubject(IGrouping<string, Requirement> group, int level)
    {
        var ofSubject = group.ToArray();
        var shared = Requirement.Shared(ofSubject);
        return Over(new(group.Key, group.Key.AsHeading(), level, shared,
            Requirement.SubjectOf(ofSubject), Requirement.ReturnTypeOf(ofSubject),
            [.. ToBranches(ofSubject.Select(requirement => requirement.Without(shared)), level + 1)],
            Requirements: []));
    }

    /// <summary>
    /// A node over the branches below it. One branch that heads nothing is not a level of its own,
    /// so what it holds is held here instead; where there are several, a requirement every one of
    /// them repeats was written here and is listed here.
    /// </summary>
    private static DocumentNode Over(DocumentNode node)
    {
        if (node.Children is [{ Heading: null } lone])
            return node with { Children = lone.Children, Requirements = lone.Requirements };

        var repeated = Requirement.Repeated(node.Children);
        return node with
        {
            Children = [.. node.Children.Select(branch => branch.Without(repeated))],
            Requirements = repeated,
        };
    }

    /// <summary>
    /// A branch path heads twice where there is depth left for it, and reads as one sentence where
    /// there is not. The second heading is what lets a clause every branch below states rise to the
    /// one above them, which a flattened path has no level to hold.
    /// </summary>
    private static IEnumerable<DocumentNode> ToBranches(IEnumerable<Requirement> ofSubject, int level)
        => level < MaxLevel
            ? ofSubject.GroupBy(requirement => Opening(requirement.Entry.Branch))
                .Select(opening => ToBranchGroup(opening, level))
            : ofSubject.GroupBy(requirement => requirement.Entry.Branch)
                .Select(branch => ToBranch(branch, level));

    private static DocumentNode ToBranchGroup(IGrouping<string, Requirement> group, int level)
    {
        var ofGroup = group.ToArray();
        var heads = group.Key.Length > 0;
        var shared = heads ? Requirement.Shared(ofGroup) : [];
        return Over(new(group.Key, heads ? group.Key.AsHeading() : null, level, shared,
            SubjectUnderTest: null, ReturnType: null,
            [.. ofGroup
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => Rest(requirement.Entry.Branch))
            .Select(branch => ToBranch(branch, level + 1))],
            Requirements: []));
    }

    private static string Opening(string branch)
        => branch.IndexOf('.') is var at && at < 0 ? branch : branch[..at];

    private static string Rest(string branch)
        => branch.IndexOf('.') is var at && at < 0 ? string.Empty : branch[(at + 1)..];

    private static DocumentNode ToBranch(IGrouping<string, Requirement> group, int level)
    {
        var ofBranch = group.ToArray();
        var heads = group.Key.Length > 0;
        var shared = heads ? Requirement.Shared(ofBranch) : [];
        return new(group.Key, heads ? group.Key.AsHeading() : null, level, shared,
            [.. ofBranch.Select(requirement => requirement.Without(shared))]);
    }

    private static int CommonRootDepth(Requirement[] requirements)
    {
        var paths = requirements.Select(requirement => Segments(requirement.Entry.Namespace)).ToArray();
        if (paths.Length == 0)
            return 0;

        var first = paths[0];
        var depth = first.Length;
        foreach (var path in paths)
            depth = CommonPrefixDepth(first, path, depth);
        return depth;
    }

    private static int CommonPrefixDepth(string[] first, string[] second, int limit)
    {
        var common = 0;
        while (common < limit && common < second.Length && second[common] == first[common])
            common++;
        return common;
    }

    private static string AreaOf(string? @namespace, int rootDepth)
    {
        var segments = Segments(@namespace);
        return segments.Length > rootDepth ? segments[rootDepth] : string.Empty;
    }

    private static string GroupOf(string? @namespace, int depth)
        => string.Join('.', Segments(@namespace).Skip(depth));

    private static string[] Segments(string? @namespace)
        => @namespace?.Split('.', StringSplitOptions.RemoveEmptyEntries) ?? [];
}