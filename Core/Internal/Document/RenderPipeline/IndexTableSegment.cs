using System.Globalization;

namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// One row per file of the specification: the file as a link, and how many acts, cases and
/// requirements it holds — each counted as the file shows them. A last row sums them all, in
/// italics so it reads as a different kind of row.
/// </summary>
internal sealed record IndexTableSegment(IReadOnlyList<Document> Documents) : DocumentSegment
{
    private static readonly string[] _headers = ["Specification", "When", "Given", "Then"];

    /// The columns holding a count, which read best right-aligned.
    private const int FirstCount = 1;

    internal override string Render()
    {
        string[][] rows = [_headers, .. Documents.Select(Row), All()];
        var widths = Enumerable.Range(0, _headers.Length)
            .Select(column => rows.Max(row => row[column].Length))
            .ToArray();
        return "\n"
            + Line(rows[0], widths)
            + Rule(widths)
            + string.Concat(rows.Skip(1).Select(row => Line(row, widths)))
            + "\n";
    }

    private static string[] Row(Document document) => [
        $"[{document.Name}]({SpecificationFile.FileNameOf(document.Name)})",
        Count(document.Whens), Count(document.Givens), Count(document.Thens)];

    private string[] All() => [
        Italic("All"),
        Italic(Count(Documents.Sum(document => document.Whens))),
        Italic(Count(Documents.Sum(document => document.Givens))),
        Italic(Count(Documents.Sum(document => document.Thens)))];

    private static string Italic(string text) => $"*{text}*";

    private static string Count(int count) => count.ToString(CultureInfo.InvariantCulture);

    /// Dashes over the cell and its margins, ending in a colon where the column is right-aligned.
    private static string Rule(int[] widths)
        => "|"
            + string.Concat(widths.Select((width, column) =>
                column < FirstCount ? $"{new string('-', width + 2)}|" : $"{new string('-', width + 1)}:|"))
            + "\n";

    private static string Line(string[] row, int[] widths)
        => "|"
            + string.Concat(row.Select((cell, column) =>
                $" {(column < FirstCount ? cell.PadRight(widths[column]) : cell.PadLeft(widths[column]))} |"))
            + "\n";
}
