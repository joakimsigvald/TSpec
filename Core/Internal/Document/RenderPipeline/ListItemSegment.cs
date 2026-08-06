using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record ListItemSegment(string Name, ComposedText Claim) : DocumentSegment
{
    private const int ItemIndentation = 2;
    private const int FenceWidth = Document.Width - ItemIndentation;
    private const int ClaimWidth = FenceWidth - 2;

    internal override string Render()
    {
        var claim = Claim.Fit(ClaimWidth);
        if (claim.Contains('\n'))
            return $"- **{Name}**\n{Indent($"```\n{Claim.Fit(FenceWidth)}\n```")}\n";

        var beside = $"- **{Name}** — `{claim}`";
        return beside.Length <= Document.Width
            ? $"{beside}\n"
            : $"- **{Name}**\\\n{Indent($"`{claim}`")}\n";
    }

    private static string Indent(string block)
        => string.Join("\n",
            block.Split('\n').Select(line => $"{new string(' ', ItemIndentation)}{line}"));
}
