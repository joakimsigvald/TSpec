using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// One file of the specification: the requirements of one top-level folder of the spec project,
/// or of what sits at its root, under a title naming it. What every requirement in the file states
/// is stated once at its top, and the root node holds that along with the groups below it.
/// </summary>
internal sealed record Document(
    SpecificationSubject Subject,
    string SpecAssemblyName,
    string Name,
    DocumentNode Root,
    string? SourceRoot)
{
    internal const int Width = 90;

    internal string Title => Root.Heading!;

    /// The spec assembly, and the folder of it a folder's file was generated from.
    internal string GeneratedFrom
        => Root.HasKey ? $"{SpecAssemblyName}/{Root.Key}" : SpecAssemblyName;

    // ----------- What the index says of the file: counted as the file shows them

    /// The acts, one heading each below whatever group holds them.
    internal int Whens => Root.Children.Sum(group => group.Children.Count);

    /// The cases, each heading a nested given takes counted.
    internal int Givens
        => Root.Children.SelectMany(group => group.Children).Sum(subject => Headed(subject.Children));

    /// The requirements listed, a theory once and a repeated requirement where it is listed.
    internal int Thens => Listed(Root);

    private static int Headed(IReadOnlyList<DocumentNode> nodes)
        => nodes.Sum(node => (node.Heading is null ? 0 : 1) + Headed(node.Children));

    private static int Listed(DocumentNode node)
        => node.Requirements.Count + node.Children.Sum(Listed);

    /// <summary>
    /// The files of the specification, by name. A folder named as the project is, or README, would
    /// take a file the specification already writes, so that fails rather than writes one over the
    /// other.
    /// </summary>
    internal static IReadOnlyList<Document> Of(
        SpecificationSubject subject, string specAssemblyName,
        IReadOnlyList<Requirement> requirements, string? sourceRoot)
    {
        var rootDepth = RootDepth(requirements, specAssemblyName);
        var documents = requirements
            .GroupBy(requirement => AreaOf(requirement.Entry.Namespace, rootDepth))
            .Select(area => ToDocument(area, rootDepth, subject, specAssemblyName, sourceRoot))
            .OrderBy(document => document.Name, StringComparer.Ordinal)
            .ToArray();
        var clash = documents.Select(document => document.Name)
            .Append(IndexRenderer.Name)
            .GroupBy(name => name)
            .FirstOrDefault(name => name.Count() > 1);
        if (clash is not null)
            throw new SetupFailed(
                $"TSpec cannot write the specification: a folder of {specAssemblyName} is named "
                + $"'{clash.Key}', which is a file the specification already writes. Rename the folder.");
        return documents;
    }

    /// Where the documents' links to source point, from the folder the documents sit in.
    internal string? Href(SourceLocation? at)
        => SourceLink.Href(at, SourceRoot) is { } relative ? $"../{relative}" : null;

    private const int RootLevel = 1;
    private const int GroupLevel = 2;
    private const int MaxLevel = 4;

    private static Document ToDocument(
        IGrouping<string, Requirement> area, int rootDepth,
        SpecificationSubject subject, string specAssemblyName, string? sourceRoot)
    {
        var ofArea = area.ToArray();
        var atRoot = area.Key.Length == 0;
        var name = atRoot ? LastPart(subject.Name) : area.Key;
        var title = atRoot ? name.AsTitle() : area.Key.AsHeading();
        var shared = Requirement.Shared(ofArea, acts: false);
        var groups = ofArea
            .Select(requirement => requirement.Without(shared))
            .GroupBy(requirement => GroupOf(requirement.Entry.Namespace, rootDepth + 1))
            .ToArray();
        DocumentNode root = new(area.Key, title, RootLevel, shared,
            Requirement.SubjectOf(ofArea), Requirement.ReturnTypeOf(ofArea),
            [.. groups.Select(group => ToGroup(group, heads: groups.Length > 1))],
            Requirements: []);
        return new(subject, specAssemblyName, name, root, sourceRoot);
    }

    private static string LastPart(string subjectName) => subjectName[(subjectName.LastIndexOf('.') + 1)..];

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
            Requirements: [],
            ofSubject[0].Entry.Source));
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

    /// <summary>
    /// The project's own namespace is the root the folders sit under, so a folder is a file even
    /// when it is the only one. Specs that do not sit under it have no project to be relative to,
    /// and there what they share stands in for it.
    /// </summary>
    private static int RootDepth(IReadOnlyList<Requirement> requirements, string specAssemblyName)
    {
        var paths = requirements.Select(requirement => Segments(requirement.Entry.Namespace)).ToArray();
        var project = Segments(specAssemblyName);
        return paths.Length > 0 && paths.All(path => CommonPrefixDepth(project, path, project.Length) == project.Length)
            ? project.Length
            : CommonRootDepth(paths);
    }

    private static int CommonRootDepth(string[][] paths)
    {
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
