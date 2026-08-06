using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// One part of the document, as the layout settled it. What it says is here; how wide it comes out
/// is not, so <see cref="Content"/> stays composed rather than laid out to any width.
/// </summary>
internal sealed record DocumentSegment(
    DocumentStyle Style,
    string? Text = null,
    int Level = 0,
    ComposedText? Content = null)
{
    internal static DocumentSegment Ruler => new(DocumentStyle.Ruler);
}
