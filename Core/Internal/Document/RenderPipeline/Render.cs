using System.Text;
using TSpec.Internal.Specification;
using static TSpec.Internal.Document.RenderPipeline.DocumentText;

namespace TSpec.Internal.Document.RenderPipeline;

internal class Render(DocumentSegment[] segments)
{
    private StringBuilder? _text;
    public override string ToString() => (_text ??= Build()).ToString();

    private StringBuilder Build()
    {
        StringBuilder text = new();
        foreach (var segment in segments)
            text.Append(Rendered(segment));
        return text;
    }

    private static string Rendered(DocumentSegment segment) => segment.Style switch
    {
        DocumentStyle.Code => Block(segment.Text!),
        DocumentStyle.Heading => $"\n{new string('#', segment.Level)} {segment.Text}\n",
        DocumentStyle.Ruler => "\n---\n",
        DocumentStyle.Item => Item(segment.Text!, segment.Content!),
        _ => segment.Text!,
    };

    /// <summary>
    /// A requirement as a list item. The claim stands beside its name where both fit on a line,
    /// fences where it needed more than one, and otherwise breaks under the name — hard, since a
    /// soft break would be reflowed away and put back the line it broke.
    /// </summary>
    private static string Item(string label, ComposedText content)
    {
        var claim = Fit(content, ClaimWidth);
        if (claim.Contains('\n'))
            return $"- **{label}**\n{Indent($"```\n{Fit(content, FenceWidth)}\n```")}\n";

        var beside = $"- **{label}** — `{claim}`";
        return beside.Length <= DocumentWidth
            ? $"{beside}\n"
            : $"- **{label}**\\\n{Indent($"`{claim}`")}\n";
    }
}
