using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

internal sealed record ListItemSegment(string Name, ComposedText Claim) : DocumentSegment
{
    private const int ItemIndentation = 2;
    private const int FenceWidth = Document.Width - ItemIndentation;
    private const int ClaimWidth = FenceWidth - 2;
    private const string ToDoHint = "*TODO: Assert behaviour*";
    private readonly string _bulletStart = $"- **{Name}**";

    internal override string Render() => _bulletStart + RenderClaim(Claim.Fit(ClaimWidth));

    private string RenderClaim(string claim)
        => string.IsNullOrEmpty(claim) ? Beside(ToDoHint)
            : claim.Contains('\n') ? $"\n{Indent(Blocked(Claim.Fit(FenceWidth)))}\n"
            : Beside($"`{claim}`");

    private static string Blocked(string content) => $"```\n{content}\n```";

    private string Beside(string claim)
        => claim.Length <= Document.Width - _bulletStart.Length - 2 ? $" — {claim}\n" : $"\\\n{Indent(claim)}\n";

    private static string Indent(string block)
        => string.Join("\n",
            block.Split('\n').Select(line => $"{new string(' ', ItemIndentation)}{line}"));
}