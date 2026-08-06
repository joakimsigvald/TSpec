using TSpec.Internal.Specification;

namespace TSpec.Internal.Document;

/// <summary>
/// What the document is made of, settled before a character of it is written. The renderer reads
/// this and nothing else, and never writes back to it.
/// </summary>
/// <remarks>
/// It carries every decision that can be made without measuring rendered text — what heads a
/// section, what it declares, what runs under it. Choices that depend on how wide something turns
/// out to be stay with the renderer, since width is the renderer's variable, and so does order:
/// what a document contains is structure, the sequence it reads in is presentation.
/// <para>
/// That is also the rule for what may be stored here: composed text, never text that has been
/// through <see cref="DocumentText.Fit"/>.
/// </para>
/// <para>
/// <c>Whole</c> is what every requirement in the document states, which the document says once of
/// itself. The act is left out of it: nothing above a subject is named after the act.
/// </para>
/// </remarks>
internal sealed record Document(
    SpecificationSubject Subject,
    string SpecAssemblyName,
    string BuildId,
    Declared Declared,
    IReadOnlyList<SpecificationClause> Whole,
    IReadOnlyList<DocumentNode> Areas)
{
    internal static Document Of(
        SpecificationSubject subject, string specAssemblyName, string buildId,
        IEnumerable<SpecificationEntry> entries)
    {
        Requirement[] requirements = [.. Requirement.From(entries)];
        var whole = Requirement.Shared(requirements, acts: false);
        return new(subject, specAssemblyName, buildId,
            Declared.Of(requirements, returns: false), whole, ToAreas(requirements, whole));
    }

    /// An area heads at the title's own level, there being none above it.
    private const int AreaLevel = 1;

    private const int GroupLevel = AreaLevel + 1;

    /// <summary>
    /// The areas of the system — one per namespace segment the specs differ in, which is the folder
    /// they were written in. Specs at the shared root belong to no area and keep the title as their
    /// heading, so they head nothing of their own.
    /// </summary>
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

    /// <summary>
    /// One group of an area and the subjects under it. A lone group heads nothing — the area above
    /// it already divides exactly what it would — so it states nothing of its own either, and what
    /// it would have headed runs at the level it did not take.
    /// </summary>
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

    /// <summary>
    /// How many leading namespace segments every spec shares. Below that is where they differ, and
    /// the segment right below it names an area — so a document whose specs share one namespace has
    /// no areas at all, which is right twice over: nothing to tell apart, and a heading spanning
    /// everything states nothing.
    /// </summary>
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

    /// The area a spec belongs to: the first namespace segment below the shared root.
    private static string AreaOf(string? @namespace, int rootDepth)
    {
        var segments = Segments(@namespace);
        return segments.Length > rootDepth ? segments[rootDepth] : string.Empty;
    }

    /// <summary>
    /// The group a spec belongs to within its area: whatever is left of its namespace below the
    /// area, as one dotted key. Merged rather than nested a segment at a time — the levels a
    /// document can tell apart are nearly spent by the area, and what is left names one thing.
    /// </summary>
    private static string GroupOf(string? @namespace, int depth)
        => string.Join('.', Segments(@namespace).Skip(depth));

    private static string[] Segments(string? @namespace)
        => @namespace?.Split('.', StringSplitOptions.RemoveEmptyEntries) ?? [];
}
