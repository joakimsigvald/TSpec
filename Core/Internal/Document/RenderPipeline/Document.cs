using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record Document(
    SpecificationSubject Subject,
    string SpecAssemblyName,
    string BuildId,
    Declared Declared,
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
            Declared.Of(requirements, returns: false), whole, ToAreas(requirements, whole));
    }

    private const int AreaLevel = 1;

    private const int GroupLevel = AreaLevel + 1;

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
        var declared = heads ? Declared.Of(ofArea, returns: false) : default;
        var groups = ofArea
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => GroupOf(requirement.Entry.Namespace, rootDepth + 1))
            .ToArray();
        return new(area.Key, heads ? area.Key.AsHeading() : null, AreaLevel, shared, declared,
            [.. groups.Select(group => ToGroup(group, heads: groups.Length > 1))]);
    }

    private static DocumentNode ToGroup(IGrouping<string, Requirement> group, bool heads)
    {
        var ofGroup = group.ToArray();
        var shared = heads ? Requirement.Shared(ofGroup, acts: false) : [];
        var declared = heads ? Declared.Of(ofGroup, returns: false) : default;
        var subjectLevel = heads ? GroupLevel + 1 : GroupLevel;
        return new(group.Key, heads ? group.Key.AsTitle() : null, GroupLevel, shared, declared,
            [.. ofGroup
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => requirement.Entry.Subject)
            .Select(subject => ToSubject(subject, subjectLevel))]);
    }

    private static DocumentNode ToSubject(IGrouping<string, Requirement> group, int level)
    {
        var ofSubject = group.ToArray();
        var shared = Requirement.Shared(ofSubject);
        return new(group.Key, group.Key.AsHeading(), level, shared, Declared.Of(ofSubject),
            [.. ofSubject
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => requirement.Entry.Branch)
            .Select(branch => ToBranch(branch, level + 1))]);
    }

    private static BranchNode ToBranch(IGrouping<string, Requirement> group, int level)
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