using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using TSpec.Internal.Document;

namespace TSpec.Architecture.Internal;

/// <summary>
/// The files a project compiles from its own folder, read from the debug information beside its
/// assembly. That names every file the compiler read, one that declares only an interface too, but
/// also what the build generated under obj/ and what was linked in from elsewhere, such as the files
/// a test framework's package compiles into a spec project.
/// </summary>
internal static class ProjectSources
{
    private static readonly ConcurrentDictionary<(string Project, string Assembly), IReadOnlyList<SourceFile>> _files = new();

    // The same files every time: an extension member keeps what it computes against the file it was given.
    internal static IReadOnlyList<SourceFile> Of(string project, string assembly)
        => _files.GetOrAdd((project, assembly), _ => Read(project, assembly));

    internal static IReadOnlyList<SourceFile> From(string project, IReadOnlyCollection<string> documents)
    {
        var folder = ProjectDirectory.Holding(documents) ?? throw new SetupFailed(
            $"TSpec could not read the sources of '{project}': they are not where its debug information says, "
            + "as when a build maps source paths.");
        return [.. documents
            .Select(document => (Location: document, Path: InProject(folder, document)))
            .Where(file => IsInside(file.Path) && !IsGenerated(file.Path))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .Select(file => new SourceFile(project, file.Path, file.Location))];
    }

    private static IReadOnlyList<SourceFile> Read(string project, string assembly)
    {
        if (!SourceLocations.TryOpenDebugInformation(assembly, out var provider))
            throw new SetupFailed(
                $"TSpec could not read the sources of '{project}': there is no debug information for '{assembly}'.");

        using (provider)
        using (var image = new PEReader(File.OpenRead(assembly)))
        {
            var debug = provider.GetMetadataReader();
            var files = From(project, [.. debug.Documents.Select(document => debug.GetString(debug.GetDocument(document).Name))]);
            Declare(new TypeDeclarations(image.GetMetadataReader(), debug).All, files);
            return files;
        }
    }

    // One object per type, so a partial type is the same type on each of its files.
    private static void Declare(IEnumerable<TypeDeclaration> declarations, IReadOnlyList<SourceFile> files)
    {
        var byLocation = files.ToDictionary(file => file.Location);
        var types = declarations
            .Select(declaration => Declared(declaration, byLocation))
            .Where(type => type.Files.Count > 0)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();
        foreach (var file in files)
            file.Types = [.. types.Where(type => type.Files.Contains(file))];
    }

    private static DeclaredType Declared(TypeDeclaration declaration, Dictionary<string, SourceFile> byLocation)
        => new(declaration.Name, declaration.TypeParameters, declaration.Namespace, declaration.Kind, declaration.IsPublic,
            [.. declaration.Documents
                .Select(byLocation.GetValueOrDefault)
                .OfType<SourceFile>()
                .OrderBy(file => file.Path, StringComparer.Ordinal)]);

    private static string InProject(string folder, string document)
        => Path.GetRelativePath(folder, document).Replace(Path.DirectorySeparatorChar, '/');

    private static bool IsInside(string path) => !path.StartsWith("../", StringComparison.Ordinal) && !Path.IsPathRooted(path);

    private static bool IsGenerated(string path) => path.StartsWith("obj/", StringComparison.Ordinal);
}
