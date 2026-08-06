using TSpec.Internal.Specification;
using static TSpec.Internal.Document.RenderPipeline.DocumentText;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record ListItemSegment(string Name, ComposedText Claim) : DocumentSegment
{
    internal override string Render()
    {
        var claim = Claim.Fit(ClaimWidth);
        if (claim.Contains('\n'))
            return $"- **{Name}**\n{Indent($"```\n{Claim.Fit(FenceWidth)}\n```")}\n";

        var beside = $"- **{Name}** — `{claim}`";
        return beside.Length <= DocumentWidth
            ? $"{beside}\n"
            : $"- **{Name}**\\\n{Indent($"`{claim}`")}\n";
    }

    internal static string Indent(string block)
        => string.Join("\n",
            block.Split('\n').Select(line => $"{new string(' ', ItemIndentation)}{line}"));
}
