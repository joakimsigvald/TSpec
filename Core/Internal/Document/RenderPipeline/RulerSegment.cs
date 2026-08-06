namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record RulerSegment : DocumentSegment
{
    internal override string Render() => "\n---\n";
}
