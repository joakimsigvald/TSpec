using TSpec.Architecture;

namespace TSpec.Internal.Document.RenderPipeline;

/// The projects as a Mermaid flowchart, packages left out. Each project and each reference is a line
/// of its own, in ordinal order, so a change to the graph diffs line by line.
internal sealed record ProjectGraphSegment(IReadOnlyList<ProjectNode> Nodes) : DocumentSegment
{
    // A Mermaid style cannot use the viewer's theme colours, so a spec project is set apart by what
    // reads on a light background and a dark one alike.
    private const string SpecStyle = "classDef spec fill:none,stroke-dasharray:4 3,opacity:0.8";

    internal static ProjectGraphSegment Of(SourceProjects projects, IReadOnlyDictionary<string, string> indexes)
        => new(ProjectNode.All(projects, indexes));

    internal override string Render() => $"\n{Flowchart([.. Drawn(Nodes), .. Nodes.SelectMany(Link), SpecStyle])}\n";

    // The nodes and arrows alone, as drawn under a test that allows them.
    internal static string Plain(IReadOnlyList<ProjectNode> nodes) => Flowchart(Drawn(nodes));

    private static string Flowchart(IEnumerable<string> lines)
        => $"```mermaid\nflowchart TD\n{string.Concat(lines.Select(line => $"    {line}\n"))}```";

    private static IEnumerable<string> Drawn(IReadOnlyList<ProjectNode> nodes)
        => [.. nodes.Select(Declaration), .. nodes.SelectMany(Edges)];

    private static string Declaration(ProjectNode node) => node.IsSpec ? $"{Shape(node)}:::spec" : Shape(node);

    private static string Shape(ProjectNode node) => node.Label == node.Id ? node.Id : $"{node.Id}[\"{node.Label}\"]";

    private static IEnumerable<string> Edges(ProjectNode node)
        => node.References.Select(reference => $"{node.Id} {Arrow(node)} {reference}");

    private static string Arrow(ProjectNode node) => node.IsSpec ? "-.->" : "-->";

    // A link by URL is followed even at Mermaid's strict security level, which only disables scripted clicks.
    private static IEnumerable<string> Link(ProjectNode node) => node.Index is null ? [] : [$"click {node.Id} \"{node.Index}\""];
}
