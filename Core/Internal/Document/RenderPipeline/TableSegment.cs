namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// A theory's rows, tabled under the requirement they all state. The claim above says what holds
/// for every row; each row says what it filled the requirement's holes with.
/// </summary>
/// <remarks>
/// A table cannot wrap the way prose does — a broken row stops being a row — so instead of laying
/// the text out to a width, the columns divide the width between them and a value too long for its
/// share is cut. Equal shares are the simple rule, not the best one: columns sized to what each
/// needs would fit more, and is the refinement to make when a real document asks for it.
/// </remarks>
internal sealed record TableSegment(IReadOnlyList<TheoryRow> Rows) : DocumentSegment
{
    private const int Indentation = 2;

    /// A bar opens a cell and one closes it, with a space inside each: four places per column.
    private const int Furniture = 3;
    private const int NarrowestColumn = 3;
    private const char Cut = '…';

    internal override string Render()
    {
        string[][] cells = [Escaped(Rows[0].Headers), .. Rows.Select(row => Escaped(row.Values))];
        var widths = Widths(cells, Rows[0].Headers.Count);
        return "\n"
            + Line(cells[0], widths)
            + Line([.. widths.Select(width => new string('-', width))], widths)
            + string.Concat(cells.Skip(1).Select(row => Line(row, widths)))
            + "\n";
    }

    /// What each column needs, up to what it may have.
    private static int[] Widths(string[][] cells, int columns)
    {
        var share = Share(columns);
        return [.. Enumerable.Range(0, columns).Select(column =>
            Math.Min(share, cells.Max(row => column < row.Length ? row[column].Length : 0)))];
    }

    private static int Share(int columns)
        => Math.Max(
            NarrowestColumn,
            (Document.Width - Indentation - 1) / columns - Furniture);

    private static string Line(IReadOnlyList<string> row, int[] widths)
        => new string(' ', Indentation)
            + "|"
            + string.Concat(widths.Select((width, column) =>
                $" {Fit(column < row.Count ? row[column] : string.Empty, width).PadRight(width)} |"))
            + "\n";

    /// The ellipsis takes the last place, and never leaves the backslash of an escape behind it.
    private static string Fit(string text, int width)
        => text.Length <= width ? text : $"{text[..(width - 1)].TrimEnd('\\')}{Cut}";

    /// <summary>
    /// A bar would end the cell it stands in and a line break would end the row, so a value states
    /// them as text. Everything else a value says, it says as the rest of the document says it.
    /// </summary>
    private static string[] Escaped(IReadOnlyList<string> values)
        => [.. values.Select(value => value
            .Replace("|", "\\|", StringComparison.Ordinal)
            .ReplaceLineEndings(" "))];
}
