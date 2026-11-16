namespace TSpec.Internal.Document.RenderPipeline;

/// Markdown written as it is given, standing apart from what is above it.
internal sealed record MarkdownSegment(string Text) : DocumentSegment
{
    internal override string Render() => $"\n{Text}\n";
}
