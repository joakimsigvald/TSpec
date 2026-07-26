namespace TSpec.Internal.Document;

/// <summary>
/// Locates the source directory of the spec project, so the document lands next to its
/// project file rather than in the build output.
/// </summary>
internal static class ProjectDirectory
{
    internal static string Locate(string baseDirectory)
    {
        for (var directory = new DirectoryInfo(baseDirectory); directory is not null; directory = directory.Parent)
            if (directory.EnumerateFiles("*.csproj").Any())
                return directory.FullName;
        throw new SetupFailed(
            $"TSpec could not locate the spec project directory: no .csproj file in '{baseDirectory}' "
            + "or any of its parents.");
    }
}
