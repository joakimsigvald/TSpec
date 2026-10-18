using TSpec.Internal.Specification;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// One requirement as an item of a list. Its table, a theory's rows or what an assertion stated as
/// one, sits directly under the bullet, ahead of a claim that took a fence of its own: what the rows
/// were is read before what is claimed of them, and a claim standing on the bullet's own line is
/// read before either.
/// </summary>
internal sealed record ListItemSegment(
    string Name, ComposedText Claim, SpecificationTable? Table) : DocumentSegment
{
    private const int ItemIndentation = 2;
    private const int FenceWidth = Document.Width - ItemIndentation;
    private const int ClaimWidth = FenceWidth - 2;
    private const string ToDoHint = "*TODO: Assert behaviour*";
    private readonly string _bulletStart = $"- **{Name}**";

    internal override string Render()
    {
        var claim = Claim.Fit(ClaimWidth);
        return Fenced(claim)
            ? $"{_bulletStart}\n{Tabled()}{Indent(Blocked(Claim.Fit(FenceWidth)))}\n"
            : _bulletStart + Beside(string.IsNullOrEmpty(claim) ? ToDoHint : $"`{claim}`") + Tabled();
    }

    private static bool Fenced(string claim) => claim.Contains('\n');

    private string Tabled() => Table is null ? string.Empty : $"{Drawn(Table)}{new TableSegment(Table).Render()}";

    private static string Drawn(SpecificationTable table)
        => table.Arrows is null
            ? string.Empty
            : $"\n{Indent(ProjectGraphSegment.Plain(ProjectNode.Allowed(table.Rows.Select(row => row[0]), table.Arrows)))}\n";

    private static string Blocked(string content) => $"```\n{content}\n```";

    private string Beside(string claim)
        => claim.Length <= Document.Width - _bulletStart.Length - 2 ? $" — {claim}\n" : $"\\\n{Indent(claim)}\n";

    private static string Indent(string block)
        => string.Join("\n",
            block.Split('\n').Select(line => $"{new string(' ', ItemIndentation)}{line}"));
}
