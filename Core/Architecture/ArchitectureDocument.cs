using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TSpec.Internal.Document;
using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Architecture;

/// <summary>
/// Writes the architecture of the solution to _specification.md in the spec project: the projects it
/// was built with and their references, as a graph. Enable it with one line in the spec project:
/// <code>[assembly: AssemblyFixture(typeof(ArchitectureDocument))]</code>
/// Only a run in which every test of a <see cref="Spec"/> class passed rewrites it.
/// </summary>
public sealed class ArchitectureDocument : IDisposable
{
    private readonly Assembly _specAssembly;
    private readonly string? _directory;

    /// <summary>
    /// Locates the spec project, or reports why it could not
    /// </summary>
    public ArchitectureDocument()
    {
        _specAssembly = CompleteRun.SpecAssemblyDeclaring(typeof(ArchitectureDocument));
        CompleteRun.Begin(
            ArchitectureRenderer.FileName,
            located: ProjectDirectory.TryLocate(SpecClasses.SourcesOf(_specAssembly), AppContext.BaseDirectory, out _directory));
    }

    /// <summary>
    /// Writes the page where the run was complete and green, or names the tests that kept it from being so
    /// </summary>
    public void Dispose()
    {
        var directory = _directory;
        if (directory is null)
            return;

        CompleteRun.End(ArchitectureRenderer.FileName, _specAssembly, entries => Write(directory, entries));
    }

    private void Write(string directory, IReadOnlyCollection<SpecificationEntry> entries)
        => PendingSpecification.Write(
            Path.Combine(directory, ArchitectureRenderer.FileName),
            ArchitectureRenderer.Render(Source.Projects, _specAssembly.GetName().Name!, entries, Indexes(directory)));

    // Each spec project's own index, by the spec project, as linked from this page.
    private static Dictionary<string, string> Indexes(string directory)
    {
        Dictionary<string, string> indexes = [];
        foreach (var project in Source.Projects)
            if (TryFindIndex(project, out var index))
                indexes[project.AssemblyName] = Path.GetRelativePath(directory, index).Replace('\\', '/');
        return indexes;
    }

    private static bool TryFindIndex(SourceProject project, [NotNullWhen(true)] out string? index)
    {
        index = ProjectFolder.TryFind(project, out var folder)
            ? Path.Combine(folder, SpecificationDocument.FolderName, $"{IndexRenderer.Name}.md")
            : null;
        return File.Exists(index);
    }
}
