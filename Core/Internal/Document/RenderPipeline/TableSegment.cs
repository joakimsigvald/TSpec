namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// A theory's rows, tabled under the requirement they all state. The claim above says what holds
/// for every row; each row says what it filled the requirement's holes with.
/// </summary>
/// <remarks>
/// A table cannot wrap the way prose does — a broken row stops being a row — so instead of laying
/// the text out to a width, the columns divide the width between them and a value too long for its
/// share is cut.
/// </remarks>
internal sealed record TableSegment(IReadOnlyList<TheoryRow> Rows) : DocumentSegment
{
    private const int Indentation = 2;

    /// A bar opens a cell and one closes it, with a space inside each: four places per column.
    private const int Furniture = 3;
    private const int NarrowestColumn = 5;
    private const char Cut = '…';

    /// <summary>
    /// What the page holds with every column still readable. Ten would fit at the narrowest a
    /// column may be; eight leaves room to breathe, and a theory wanting more has more parameters
    /// than a reader can hold in their head anyway.
    /// </summary>
    private const int MostColumns = 8;

    internal override string Render()
    {
        Fits(Rows[0].Headers);
        string[][] cells = [Escaped(Rows[0].Headers), .. Rows.Select(row => Escaped(row.Values))];
        var widths = Widths(cells, Rows[0].Headers.Count);
        return "\n"
            + Line(cells[0], widths)
            + Line([.. widths.Select(width => new string('-', width))], widths)
            + string.Concat(cells.Skip(1).Select(row => Line(row, widths)))
            + "\n";
    }

    private static void Fits(IReadOnlyList<string> headers)
    {
        if (headers.Count <= MostColumns)
            return;
        throw new SetupFailed(
            $"TSpec cannot table a theory of {headers.Count} parameters ({string.Join(", ", headers)}): "
            + $"a row states at most {MostColumns} and stays readable. Group the parameters into a "
            + "type, or feed the theory from MemberData, which is not tabled.");
    }

    /// <summary>
    /// What each column needs, up to what it may have. A column that needs less than an equal share
    /// takes only what it needs and leaves the rest to the columns that have more to say — so the
    /// narrowest are settled first, and every one after them divides what is still unspoken for.
    /// </summary>
    private static int[] Widths(string[][] cells, int columns)
    {
        var needed = Enumerable.Range(0, columns)
            .Select(column => cells.Max(row => column < row.Length ? row[column].Length : 0))
            .ToArray();
        var widths = new int[columns];
        var unspokenFor = Document.Width - Indentation - 1 - Furniture * columns;
        var remaining = columns;
        foreach (var column in Enumerable.Range(0, columns).OrderBy(column => needed[column]))
        {
            var share = Math.Max(NarrowestColumn, unspokenFor / remaining);
            widths[column] = Math.Min(needed[column], share);
            unspokenFor -= widths[column];
            remaining--;
        }
        return widths;
    }

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
