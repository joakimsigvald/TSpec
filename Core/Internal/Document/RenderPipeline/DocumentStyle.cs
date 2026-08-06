namespace TSpec.Internal.Document.RenderPipeline;

internal enum DocumentStyle
{
    /// Markdown the layout assembled itself, emitted as it stands.
    Raw,

    /// Specification text, which Render quotes inline or fences depending on how it came out.
    Code,

    /// A section heading at <see cref="DocumentSegment.Level"/>.
    Heading,

    Ruler,

    /// <summary>
    /// One requirement as an item of a list: <see cref="DocumentSegment.Text"/> names it and
    /// <see cref="DocumentSegment.Content"/> is what it claims. Whether the claim stands beside the
    /// name, breaks under it, or fences is Render's to decide, since only it knows the widths.
    /// </summary>
    Item,
}
