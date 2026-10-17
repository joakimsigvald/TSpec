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
        if (ProjectDirectory.TryLocate(SpecClasses.SourcesOf(_specAssembly), AppContext.BaseDirectory, out _directory))
            SpecificationCollector.IsActive = true;
        else
            Console.Error.WriteLine(ProjectDirectory.Unlocatable(AppContext.BaseDirectory));
    }

    /// <summary>
    /// Writes the page where the run was complete and green, or names the tests that kept it from being so
    /// </summary>
    public void Dispose()
    {
        if (_directory is null)
            return;

        var missing = CompleteRun.Missing(_specAssembly);
        if (missing.Length == 0)
            Write(_directory);
        else
            Console.Error.WriteLine(CompleteRun.Report(ArchitectureRenderer.FileName, missing));
    }

    private void Write(string directory)
        => PendingSpecification.Write(
            Path.Combine(directory, ArchitectureRenderer.FileName),
            ArchitectureRenderer.Render(Source.Projects, _specAssembly.GetName().Name!));
}
