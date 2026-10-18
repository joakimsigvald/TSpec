using TSpec.Architecture;

namespace TSpec.Internal.Document.RenderPipeline;

/// A project as the diagram draws it: its id and label, whether it is a spec project, the ids of the
/// projects it references, and the index of the specification that describes it. Under a test, a node
/// is a row of what the test allows, and its references are the rows it may depend on.
internal sealed record ProjectNode(string Id, string Label, bool IsSpec, IReadOnlyList<string> References, string? Index)
{
    internal static IReadOnlyList<ProjectNode> Allowed(IEnumerable<string> names, IReadOnlyList<(string From, string To)> arrows)
        => [.. names.Select(name => new ProjectNode(
            IdOf(name),
            name,
            IsSpec: false,
            [.. arrows.Where(arrow => arrow.From == name).Select(arrow => IdOf(arrow.To))],
            Index: null))];

    internal static IReadOnlyList<ProjectNode> All(SourceProjects projects, IReadOnlyDictionary<string, string> indexes)
    {
        var graph = projects.Graph;
        var indexOfSpecified = IndexOfSpecified(graph, indexes);
        return [.. graph.ProjectsButTheSpec
            .OrderBy(projects.AsWritten, StringComparer.Ordinal)
            .Select(project => new ProjectNode(
                IdOf(projects, project),
                projects.Shown(project),
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

    // A Mermaid id is letters, digits and underscores; the label keeps the name as shown.
    private static string IdOf(SourceProjects projects, string project) => IdOf(projects.Shown(project));

    private static string IdOf(string shown) => new([.. shown.Select(c => char.IsLetterOrDigit(c) ? c : '_')]);
}
