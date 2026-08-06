namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record NewLineSegment : DocumentSegment
{
    internal override string Render() => "\n";
}
