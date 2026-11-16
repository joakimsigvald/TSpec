using TSpec.Architecture.CSharp.Internal;
using TSpec.Extensibility;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// Writes the architecture of the solution to _specification.md in the spec project: the projects it
/// was built with and their references, as a graph, and the architecture tests below it. Enable it with
/// one line in the spec project:
/// <code>[assembly: AssemblyFixture(typeof(TSpec.Architecture.CSharp.ArchitectureDocument))]</code>
/// Only a run in which every test of a <see cref="Spec"/> class passed rewrites it.
/// </summary>
public sealed class ArchitectureDocument : SpecificationPage
{
    private const string FileName = "_specification.md";

    /// <summary>
    /// Locates the spec project, or reports why it could not
    /// </summary>
    public ArchitectureDocument() : base(FileName) { }

    /// <inheritdoc/>
    protected override string Subject => SubjectOf(Source.Projects, SpecAssemblyName);

    /// <inheritdoc/>
    protected override string Header(string directory) => ProjectDiagram.Of(Source.Projects, Indexes(directory));

    // The solution the projects are named under, or the spec project where they share no root.
    internal static string SubjectOf(SourceProjects projects, string specAssemblyName)
        => projects.Root is { } root ? root.Segment(^1) : specAssemblyName;

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
