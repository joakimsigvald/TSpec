namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record HeadingSegment(string Heading, int Level) : DocumentSegment
{
    internal override string Render() => $"\n{new string('#', Level)} {Heading}\n";
}
