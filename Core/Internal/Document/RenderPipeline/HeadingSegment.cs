namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record HeadingSegment(string Heading, int Level, string? Href = null) : DocumentSegment
{
    internal override string Render() => $"\n{new string('#', Level)} {Href.Around(Heading)}\n";
}
