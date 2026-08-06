namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record CommentSegment(string Text) : DocumentSegment
{
    internal override string Render() => $"\n<!-- {Text} -->\n";
}
