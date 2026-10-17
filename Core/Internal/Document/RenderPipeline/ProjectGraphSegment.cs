using TSpec.Architecture;

namespace TSpec.Internal.Document.RenderPipeline;

/// The projects as a Mermaid flowchart, packages left out. Each project and each reference is a line
/// of its own, in ordinal order, so a change to the graph diffs line by line.
internal sealed record ProjectGraphSegment(SourceProjects Graph) : DocumentSegment
{
    // A Mermaid style cannot use the viewer's theme colours, so a spec project is set apart by what
    // reads on a light background and a dark one alike.
    private const string SpecStyle = "classDef spec fill:none,stroke-dasharray:4 3,opacity:0.8";

    internal override string Render()
        => $"\n```mermaid\nflowchart TD\n{string.Concat(Lines.Select(line => $"    {line}\n"))}```\n";

    private IEnumerable<string> Lines
        => [.. Graph.NamesButTheSpec.Select(Node), .. Graph.NamesButTheSpec.SelectMany(Edges), SpecStyle];

    private string Node(string project)
        => Graph.IsSpecProject(project) ? $"{Shape(project)}:::spec" : Shape(project);

    private string Shape(string project)
        => Label(project) == Id(project) ? Id(project) : $"{Id(project)}[\"{Label(project)}\"]";

    private IEnumerable<string> Edges(string project)
        => Graph.ReferencesAsWritten(project)
            .Where(reference => !SourceProjects.IsPackage(reference))
            .Select(reference => $"{Id(project)} {Arrow(project)} {Id(reference)}");

    private string Arrow(string project) => Graph.IsSpecProject(project) ? "-.->" : "-->";

    // To the rules the root is "." and its spec project "Spec", which read as nothing in a diagram.
    private string Label(string project)
        => Graph.TryGetTheProjectItsNameExtends(project, out var extended) ? $"{Label(extended)}.{project.Segment(^1)}"
        : project == "." ? Graph.Root!.Segment(^1)
        : project;

    // A Mermaid id is letters, digits and underscores; the label keeps the name as written.
    private string Id(string project) => new([.. Label(project).Select(c => char.IsLetterOrDigit(c) ? c : '_')]);
}
