using System.Diagnostics.CodeAnalysis;

namespace TSpec.Internal.Document;

/// <summary>
/// Locates the source directory of the spec project, so the document lands next to its project
/// file rather than in the build output.
/// </summary>
/// <remarks>
/// The spec classes' own source files are what says where that is. A build is free to put the
/// binaries anywhere — an artifacts path outside the tree, or one nested under an unrelated
/// project — so walking up from them finds nothing, or finds the wrong project and says nothing
/// about it. The sources are fixed to the project that compiles them. Where they are not on disk
/// the build output answers instead, which is what it always did.
/// </remarks>
internal static class ProjectDirectory
{
    internal static bool TryLocate(
        IReadOnlyCollection<string> specSources, string baseDirectory,
        [NotNullWhen(true)] out string? directory)
    {
        directory = Holding(specSources) ?? Above(baseDirectory);
        return directory is not null;
    }

    /// <summary>
    /// Said when neither answers, in the shape the incomplete-run report uses: the specification
    /// is a by-product of the run and no test claims it, so nothing fails over it.
    /// </summary>
    internal static string Unlocatable(string baseDirectory)
        => $"TSpec: {SpecificationDocument.FolderName}/ not written — TSpec could not locate the spec "
        + "project directory. Its source files are not where the debug information says they are (a "
        + "build that maps source paths, or one that wrote none), and no .csproj file sits above the "
        + $"build output at '{baseDirectory}'.";

    /// <summary>
    /// The project most of the spec classes are written under. A file compiled in from somewhere
    /// else — a linked file, a shared project — is outvoted rather than followed, and the
    /// outermost project wins a tie so the answer does not turn on which class was read first.
    /// </summary>
    private static string? Holding(IReadOnlyCollection<string> specSources)
        => specSources
            .Select(file => Above(Path.GetDirectoryName(file)))
            .OfType<string>()
            .GroupBy(project => project, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(project => project.Count())
            .ThenBy(project => project.Key.Length)
            .FirstOrDefault()
            ?.Key;

    private static string? Above(string? path)
    {
        for (var directory = At(path); directory is not null; directory = directory.Parent)
            if (directory.Exists && directory.EnumerateFiles("*.csproj").Any())
                return directory.FullName;
        return null;
    }

    private static DirectoryInfo? At(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        try
        {
            return new(path);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
