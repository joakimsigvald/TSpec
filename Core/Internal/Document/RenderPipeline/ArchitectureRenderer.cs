using TSpec.Architecture;
using TSpec.Extensibility;
using TSpec.Internal.Specification;

#pragma warning disable CS0618 // The deprecated architecture page reads TSpec's deprecated project graph.

namespace TSpec.Internal.Document.RenderPipeline;

/// The page an architecture spec writes: the solution its projects are named under, the graph of
/// their references, and its tests as any specification renders them.
internal static class ArchitectureRenderer
{
    internal const string FileName = "_specification.md";

    // The page sits in the spec project's folder, sourceRoot, and links its tests' files from there.
    internal static string Render(
        SourceProjects projects, string specAssemblyName, IEnumerable<SpecificationEntry> entries,
        IReadOnlyDictionary<string, string> indexes, string? sourceRoot = null)
        => SpecificationPage.Render(
            SubjectOf(projects, specAssemblyName), specAssemblyName, ProjectGraphSegment.Of(projects, indexes).Text,
            entries, sourceRoot);

    internal static string SubjectOf(SourceProjects projects, string specAssemblyName)
        => projects.Root is { } root ? NameSegments.At(root, ^1) : specAssemblyName;
}
