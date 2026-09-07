namespace TSpec.Internal.Document.RenderPipeline;

/// A paragraph of prose, standing apart from whatever is above and below it.
internal sealed record ParagraphSegment(string Text) : DocumentSegment
{
    internal override string Render() => $"\n{Text}\n\n";
}
