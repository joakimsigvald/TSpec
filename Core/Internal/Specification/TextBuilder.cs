using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Lays composed text out: line-wrapping and indentation, at the width it is given. The last phase,
/// and the only one that decides where a line ends — which is why it must be handed text nothing
/// will shorten or lengthen afterwards. Where a line breaks is ranked: an explicit break point
/// (<see cref="Wrap"/>) at the shallowest nesting wins, then whitespace, then a punctuation cue,
/// then mid-word.
/// </summary>
internal class TextBuilder(
    int maxLineLength = TextBuilder.PageWidth, int indentationSize = 2, int wrapIndentation = 3)
{
    /// How wide specification text is written when nothing indents it.
    internal const int PageWidth = 80;

    private static readonly char[] _breakAfterCues = ['.', '(', '[', '{'];
    private readonly StringBuilder _sb = new();
    private int _currentLineLength;
    private int _lineIndentation;
    private int _depth;

    /// <summary>
    /// Where a continuation sits: the step of the line it continues plus the wrap delta, so the
    /// step sequence stays self-describing — one step down is structure, more is a wrap. Every
    /// continuation of one line takes this same column; only a structural line moves it.
    /// </summary>
    private int ContinuationIndentation => _lineIndentation + wrapIndentation;

    internal void Add(TextUnit unit)
    {
        if (unit.Indentation is { } indentation)
            AddLine(unit.Text, indentation);
        else
            AddWord(unit.Text, unit.Binder);
    }

    /// <summary>
    /// The laid-out text. Its first character is capitalized because it starts a sentence — unless
    /// the sentence was started above it, which is what <paramref name="opensSentence"/> says.
    /// </summary>
    internal string Build(bool opensSentence)
    {
        var text = _sb.ToString().Trim();
        return opensSentence ? text.Capitalize() : text;
    }

    private void AddWord(string word, string binder)
    {
        if (!string.IsNullOrEmpty(word))
            AddText($"{binder}{word}");
    }

    internal StringBuilder AddText(string? text)
    {
        if (text is not null)
            AddSegment(Segment.Parse(text, ref _depth));
        return _sb;
    }

    private void AddSegment(Segment segment)
    {
        if (IsExceedingMaxLineLength(segment.Length) && FitsOnOwnLine(segment.Text)
            && _currentLineLength > 0)
        {
            AddLine(segment.Trim(), ContinuationIndentation);
            return;
        }

        if (!IsExceedingMaxLineLength(segment.Length))
        {
            _sb.Append(segment.Text);
            _currentLineLength += segment.Length;
            return;
        }

        var (first, rest) = BreakLine(segment);
        _sb.Append(first);
        _currentLineLength += first.Length;
        if (rest.Length > 0)
            AddLine(rest, ContinuationIndentation);
    }

    private void AddLine(string line, int indentation)
        => AddLine(Segment.Parse(line, ref _depth), _lineIndentation = indentation);

    private void AddLine(Segment line, int indentation)
    {
        _sb.Append(Environment.NewLine);
        _sb.Append(new string(' ', _currentLineLength = indentation * indentationSize));
        AddSegment(line);
    }

    private bool IsExceedingMaxLineLength(int length)
        => length + _currentLineLength > maxLineLength;

    /// Whether the whole text would fit on a continuation line of its own. When it would, moving it
    /// there beats breaking it: what arrives as one piece is one expression, and a break inside it
    /// orphans part of it — where an expression that fits nowhere has to be broken regardless.
    private bool FitsOnOwnLine(string text)
        => text.Trim().Length + ContinuationIndentation * indentationSize <= maxLineLength;

    private (string first, Segment rest) BreakLine(Segment segment)
    {
        var window = maxLineLength - _currentLineLength;
        var cut = MarkedCut(segment, window)
            ?? WhitespaceCut(segment.Text, window)
            ?? CueCut(segment.Text, window);
        if (cut is { } at)
            return (segment.Text[..at].TrimEnd(), segment.Slice(at));

        var first = UnbreakableStart(segment.Text[..window]);
        return (first, segment.Slice(first.Length));
    }

    /// The last break point of the shallowest rank that fits — structure outranks position.
    private static int? MarkedCut(Segment segment, int window)
    {
        BreakPoint? best = null;
        foreach (var point in segment.Points)
        {
            if (point.Index > window || IsBlankBefore(segment.Text, point.Index))
                continue;
            if (best is null || point.Rank < best.Value.Rank
                || point.Rank == best.Value.Rank && point.Index > best.Value.Index)
                best = point;
        }
        return best?.Index;
    }

    private static int? WhitespaceCut(string text, int window)
        => LastCut(text, window, (c, _) => char.IsWhiteSpace(c));

    private static int? CueCut(string text, int window)
        => LastCut(text, window, (c, next) => _breakAfterCues.Contains(c) && !IsFalseLineBreak(c, next));

    private static int? LastCut(string text, int window, Func<char, char, bool> isBreakAfter)
    {
        for (int i = Math.Min(window, text.Length) - 1; i >= 0; i--)
        {
            if (!isBreakAfter(text[i], Next(text, i)))
                continue;
            if (!IsBlankBefore(text, i + 1))
                return i + 1;
        }
        return null;
    }

    private static bool IsBlankBefore(string text, int index)
        => string.IsNullOrWhiteSpace(text[..index]);

    private static char Next(string text, int i) => i + 1 < text.Length ? text[i + 1] : ' ';

    /// A segment without break position stays on the line (breaking mid-word),
    /// unless the line is already more than half used — then everything moves
    /// to the continuation line.
    private string UnbreakableStart(string segment)
        => segment.Length < maxLineLength / 2 ? string.Empty : segment;

    private static bool IsFalseLineBreak(char c, char next)
        => c == '.' && (next == '.' || char.IsDigit(next));
}

/// One break opportunity in parsed text: the index a line may end before, and the nesting depth
/// that ranks it — shallower wins.
internal readonly record struct BreakPoint(int Index, int Rank);

/// <summary>
/// A piece of text with its <see cref="Wrap"/> markers parsed out: what remains is exactly what
/// will print, measured honestly, plus where it may break. Nesting depth is carried by the caller,
/// so a construct opened in one piece ranks points in the next.
/// </summary>
internal sealed class Segment
{
    private Segment(string text, IReadOnlyList<BreakPoint> points)
    {
        Text = text;
        Points = points;
    }

    internal string Text { get; }
    internal IReadOnlyList<BreakPoint> Points { get; }
    internal int Length => Text.Length;

    internal static Segment Parse(string raw, ref int depth)
    {
        if (raw.IndexOfAny(Wrap.Markers) < 0)
            return new(raw, []);

        var text = new StringBuilder(raw.Length);
        List<BreakPoint> points = [];
        foreach (var c in raw)
            switch (c)
            {
                case Wrap.Enter: depth++; break;
                case Wrap.Exit: depth--; break;
                case Wrap.Point: points.Add(new(text.Length, depth)); break;
                default: text.Append(c); break;
            }
        return new(text.ToString(), points);
    }

    /// The remainder from <paramref name="from"/>, trimmed, its break points reindexed; a point
    /// the cut consumed is spent.
    internal Segment Slice(int from)
    {
        var rest = Text[from..].TrimStart();
        var shift = Text.Length - rest.Length;
        rest = rest.TrimEnd();
        return new(rest, [.. Points
            .Select(point => point with { Index = point.Index - shift })
            .Where(point => point.Index > 0 && point.Index <= rest.Length)]);
    }

    internal Segment Trim() => Slice(0);
}
