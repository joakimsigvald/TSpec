namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The projects as a Mermaid flowchart, packages left out. Each project and each reference is a line of its
/// own, in ordinal order, so a change to the graph diffs line by line.
/// </summary>
internal static class ProjectDiagram
{
    // A Mermaid style cannot use the viewer's theme colours, so a spec project is set apart by what
    // reads on a light background and a dark one alike.
    private const string SpecStyle = "classDef spec fill:none,stroke-dasharray:4 3,opacity:0.8";

    internal static string Of(SourceProjects projects, IReadOnlyDictionary<string, string> indexes)
        => Flowchart(NodesOf(projects, indexes));

    private static string Flowchart(IReadOnlyList<Node> nodes)
        => $"```mermaid\nflowchart TD\n{string.Concat(LinesOf(nodes).Select(line => $"    {line}\n"))}```";

    private static IEnumerable<string> LinesOf(IReadOnlyList<Node> nodes)
        => [.. nodes.Select(Declaration), .. nodes.SelectMany(Edges), .. nodes.SelectMany(Link), SpecStyle];

    private static string Declaration(Node node) => node.IsSpec ? $"{Shape(node)}:::spec" : Shape(node);

    private static string Shape(Node node) => node.Label == node.Id ? node.Id : $"{node.Id}[\"{node.Label}\"]";

    private static IEnumerable<string> Edges(Node node)
        => node.References.Select(reference => $"{node.Id} {Arrow(node)} {reference}");

    private static string Arrow(Node node) => node.IsSpec ? "-.->" : "-->";

    // A link by URL is followed even at Mermaid's strict security level, which only disables scripted clicks.
    private static IEnumerable<string> Link(Node node) => node.Index is null ? [] : [$"click {node.Id} \"{node.Index}\""];

    private static IReadOnlyList<Node> NodesOf(SourceProjects projects, IReadOnlyDictionary<string, string> indexes)
    {
        var graph = projects.Graph;
        var indexOfSpecified = IndexOfSpecified(graph, indexes);
        return [.. graph.ProjectsButTheSpec
            .OrderBy(projects.AsWritten, StringComparer.Ordinal)
            .Select(project => new Node(
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
    private static string IdOf(SourceProjects projects, string project)
        => new([.. projects.Shown(project).Select(c => char.IsLetterOrDigit(c) ? c : '_')]);

    /// <summary>
    /// A project as the diagram draws it: its id and label, whether it is a spec project, the ids of the
    /// projects it references, and the index of the specification that describes it.
    /// </summary>
    private sealed record Node(string Id, string Label, bool IsSpec, IReadOnlyList<string> References, string? Index);
}
