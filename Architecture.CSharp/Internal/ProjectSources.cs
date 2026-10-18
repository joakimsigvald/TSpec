using System.Collections.Concurrent;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CSharp;
using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The files a project compiles from its own folder, read from the debug information beside its
/// assembly. That names every file the compiler read, one that declares only an interface too, but
/// also what the build generated under obj/ and what was linked in from elsewhere, such as the files
/// a test framework's package compiles into a spec project.
/// </summary>
internal static class ProjectSources
{
    private static readonly ConcurrentDictionary<(string Project, string Assembly), ProjectSource> _sources = new();

    internal static IReadOnlyList<SourceFile> Of(SourceProject project) => SourceOf(project).Files;

    // The same files every time: an extension member keeps what it computes against the file it was given.
    internal static ProjectSource SourceOf(SourceProject project)
        => _sources.GetOrAdd((project.Name, project.AssemblyName), _ => Read(project));

    internal static IReadOnlyList<SourceFile> From(
        string project, IReadOnlyCollection<string> documents, CSharpParseOptions options)
    {
        if (!Build.TryFindProjectFolder(documents, out var folder))
            throw new SetupFailed(
                $"TSpec could not read the sources of '{project}': they are not where its debug information says, "
                + "as when a build maps source paths.");

        return [.. documents
            .Select(document => (Location: document, Path: InProject(folder, document)))
            .Where(file => IsInside(file.Path) && !IsGenerated(file.Path))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .Select(file => new SourceFile(project, file.Path, file.Location, options))];
    }

    private static ProjectSource Read(SourceProject project)
    {
        if (!Build.TryOpenDebugInformation(project, out var provider))
            throw new SetupFailed(
                $"TSpec could not read the sources of '{project.Name}': there is no debug information for '{project.AssemblyName}'.");

        using (provider)
        {
            var debug = provider.GetMetadataReader();
            var documents = DocumentsOf(debug);
            var options = CompilationOptions.ParseOptionsOf(project.Name, debug);
            var files = From(project.Name, documents, options);
            Declare(TypeDeclarations.In(files), files);
            return new(files, [.. documents.Except(files.Select(file => file.Location))], options);
        }
    }

    private static string[] DocumentsOf(MetadataReader debug)
        => [.. debug.Documents.Select(document => debug.GetString(debug.GetDocument(document).Name))];

    // One object per type, so a partial type is the same type on each of its files.
    private static void Declare(IEnumerable<DeclaredType> declared, IReadOnlyList<SourceFile> files)
    {
        var types = declared.OrderBy(type => type.FullName, StringComparer.Ordinal).ToList();
        foreach (var file in files)
            file.Types = [.. types.Where(type => type.Files.Contains(file))];
    }

    private static string InProject(string folder, string document)
        => Path.GetRelativePath(folder, document).Replace(Path.DirectorySeparatorChar, '/');

    private static bool IsInside(string path) => !path.StartsWith("../", StringComparison.Ordinal) && !Path.IsPathRooted(path);

    private static bool IsGenerated(string path) => path.StartsWith("obj/", StringComparison.Ordinal);
}
