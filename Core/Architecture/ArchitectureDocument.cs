using TSpec.Extensibility;
using TSpec.Internal.Document.RenderPipeline;

namespace TSpec.Architecture;

/// <summary>
/// Writes the architecture of the solution to _specification.md in the spec project: the projects it
/// was built with and their references, as a graph. Enable it with one line in the spec project:
/// <code>[assembly: AssemblyFixture(typeof(ArchitectureDocument))]</code>
/// Only a run in which every test of a <see cref="Spec"/> class passed rewrites it.
/// </summary>
[Obsolete(Source.MovedToCSharp)]
public sealed class ArchitectureDocument : SpecificationPage
{
    /// <summary>
    /// Locates the spec project, or reports why it could not
    /// </summary>
    public ArchitectureDocument() : base(ArchitectureRenderer.FileName) { }

    /// <inheritdoc/>
    protected override string Subject => ArchitectureRenderer.SubjectOf(Source.Projects, SpecAssemblyName);

    /// <inheritdoc/>
    protected override string Header(string directory) => ProjectGraphSegment.Of(Source.Projects, Indexes(directory)).Text;

    // Each spec project's own index, by the spec project, as linked from this page.
    private static Dictionary<string, string> Indexes(string directory)
    {
        Dictionary<string, string> indexes = [];
        foreach (var project in Source.Projects)
            if (Build.TryFindSpecificationIndex(project.AssemblyName, out var index))
                indexes[project.AssemblyName] = Path.GetRelativePath(directory, index).Replace('\\', '/');
        return indexes;
    }
}
