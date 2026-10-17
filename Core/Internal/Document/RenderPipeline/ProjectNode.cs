using TSpec.Architecture;

namespace TSpec.Internal.Document.RenderPipeline;

/// A project as the diagram draws it: its id and label, whether it is a spec project, the ids of the
/// projects it references, and the index of the specification that describes it.
internal sealed record ProjectNode(string Id, string Label, bool IsSpec, IReadOnlyList<string> References, string? Index)
{
    internal static IReadOnlyList<ProjectNode> All(SourceProjects projects, IReadOnlyDictionary<string, string> indexes)
    {
        var graph = projects.Graph;
        var indexOfSpecified = IndexOfSpecified(graph, indexes);
        return [.. graph.ProjectsButTheSpec
            .OrderBy(projects.AsWritten, StringComparer.Ordinal)
            .Select(project => new ProjectNode(
                IdOf(projects, project),
                LabelOf(projects, project),
                graph.IsSpecProject(project),
                [.. graph.ReferencesOf(project)
                    .Where(reference => !BuildGraph.IsPackage(reference))
                    .Select(reference => IdOf(projects, reference))],
                indexOfSpecified.GetValueOrDefault(project)))];
    }

    // A spec project's index is reached from the project it is named for.
    private static Dictionary<string, string> IndexOfSpecified(BuildGraph graph, IReadOnlyDictionary<string, string> indexes)
    {
        Dictionary<string, string> specified = [];
        foreach (var (spec, index) in indexes.OrderBy(index => index.Key, StringComparer.Ordinal))
            if (graph.TryGetTheProjectItsNameExtends(spec, out var project))
                specified.TryAdd(project, index);
        return specified;
    }

    // To the rules the root is "." and its spec project "Spec", which read as nothing in a diagram.
    private static string LabelOf(SourceProjects projects, string project)
        => projects.Graph.TryGetTheProjectItsNameExtends(project, out var extended)
            ? $"{LabelOf(projects, extended)}.{project.Segment(^1)}"
        : project == projects.Root ? project.Segment(^1)
        : projects.AsWritten(project);

    // A Mermaid id is letters, digits and underscores; the label keeps the name as written.
    private static string IdOf(SourceProjects projects, string project)
        => new([.. LabelOf(projects, project).Select(c => char.IsLetterOrDigit(c) ? c : '_')]);
}
