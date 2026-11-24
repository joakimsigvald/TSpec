using System.Text;

namespace TSpec.Internal.Specification;

/// One piece of a <see cref="TextGroup"/>: text it prints, a break point, or a group nested in it.
internal abstract class TextPiece
{
    /// What the piece prints when nothing in it breaks.
    internal abstract string Flat { get; }
}

internal sealed class TextRun(string text) : TextPiece
{
    internal override string Flat => text;
}

/// A break point of the group it stands in: what follows moves one step in from the line the group
/// opened on, or lines up with that line.
internal sealed class TextBreak(bool aligns) : TextPiece
{
    internal bool Aligns => aligns;
    internal override string Flat => string.Empty;
}

/// <summary>
/// Text parsed by its <see cref="Wrap"/> markers: it prints flat, or breaks at every one of its own
/// break points while each group nested in it decides for itself. A whole piece of text is one
/// group, its outermost.
/// </summary>
internal sealed class TextGroup(IReadOnlyList<TextPiece> pieces) : TextPiece
{
    private string? _flat;

    internal IReadOnlyList<TextPiece> Pieces => pieces;
    internal override string Flat => _flat ??= string.Concat(pieces.Select(piece => piece.Flat));
    internal int Width => Flat.Length;

    /// How far the text after the piece at <paramref name="index"/> runs before this group's next
    /// break — on into what follows the group, <paramref name="trailing"/>, if none comes first.
    internal int TrailingAfter(int index, int trailing)
    {
        var width = 0;
        foreach (var piece in pieces.Skip(index + 1))
        {
            if (piece is TextBreak)
                return width;

            width += piece.Flat.Length;
        }
        return width + trailing;
    }

    internal static TextGroup Parse(string text)
    {
        Stack<List<TextPiece>> open = new();
        open.Push([]);
        StringBuilder run = new();
        foreach (var character in text)
        {
            if (!Wrap.Markers.Contains(character))
            {
                run.Append(character);
                continue;
            }

            EndRun(open.Peek(), run);
            Read(character, open);
        }
        EndRun(open.Peek(), run);
        while (open.Count > 1)
            CloseGroup(open);
        return new(open.Pop());
    }

    private static void Read(char marker, Stack<List<TextPiece>> open)
    {
        switch (marker)
        {
            case Wrap.Enter: open.Push([]); break;
            case Wrap.Exit: CloseGroup(open); break;
            case Wrap.Point: open.Peek().Add(new TextBreak(aligns: false)); break;
            case Wrap.Align: open.Peek().Add(new TextBreak(aligns: true)); break;
        }
    }

    private static void EndRun(List<TextPiece> group, StringBuilder run)
    {
        if (run.Length == 0)
            return;

        group.Add(new TextRun(run.ToString()));
        run.Clear();
    }

    /// An exit with no group open is ignored: the outermost group is the text itself.
    private static void CloseGroup(Stack<List<TextPiece>> open)
    {
        if (open.Count == 1)
            return;

        var closed = new TextGroup(open.Pop());
        open.Peek().Add(closed);
    }
}
