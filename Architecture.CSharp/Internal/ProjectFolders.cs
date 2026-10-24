namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The folders directly in a project's own, each with the others whose types its files use.
/// </summary>
internal static class ProjectFolders
{
    private const string Node = "Folder";

    internal static DependencyGraph Of(SourceProject project)
        => new(Node, project.Files
            .Where(InAFolder)
            .GroupBy(file => file.Folder)
            .ToDictionary(folder => folder.Key, folder => UsedBy(folder.Key, folder, project)));

    private static IReadOnlyList<string> UsedBy(string folder, IEnumerable<SourceFile> files, SourceProject project)
        => [.. files.SelectMany(file => file.Uses)
            .SelectMany(type => type.Files)
            .Where(file => file.Project == project.Name && InAFolder(file))
            .Select(file => file.Folder)
            .Where(other => other != folder)
            .Distinct()];

    private static bool InAFolder(SourceFile file) => file.Folder.Length > 0;
}
