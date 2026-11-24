using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Lays composed text out: line-wrapping and indentation, at the width it is given. The last phase,
/// and the only one that decides where a line ends — which is why it must be handed text nothing
/// will shorten or lengthen afterwards. Text breaks by its groups (<see cref="Wrap"/>): a group that
/// fits on its line stays whole, and one that does not breaks at every one of its own break points,
/// what it holds moving one step in. A part that broke ends its line. Text with no break point left
/// breaks at whitespace, then at a punctuation cue, then mid-word.
/// </summary>
/// <remarks>
/// The width has a tolerance: how far past it a line may run rather than break at all. A line a
/// little over costs a reader nothing; breaking it costs a second line and a seam through the
/// middle of one expression. It decides only whether a line breaks, never where — the break itself
/// always falls at the width, so a continuation is never wider than the page. A line spends the
/// tolerance once: having broken, it runs at the width until a new statement starts one afresh.
/// </remarks>
internal class TextBuilder(
    int maxLineLength = TextBuilder.PageWidth, int indentationSize = 2, int wrapIndentation = 3,
    int tolerance = 0)
{
    /// How wide specification text is written when nothing indents it.
    internal const int PageWidth = 80;

    private static readonly char[] _breakAfterCues = ['.', '(', '[', '{'];
    private readonly StringBuilder _sb = new();
    private int _currentLineLength;
    private int _statementIndentation;
    private int _lineIndentation;
    private int _breaks;
    private bool _hasBroken;
    private bool _partBroke;

    /// <summary>
    /// Where a statement continues: its step plus the wrap delta, so the step sequence stays
    /// self-describing — one step down is structure, more is a wrap. Nothing continuing the
    /// statement sits left of it.
    /// </summary>
    private int ContinuationIndentation => _statementIndentation + wrapIndentation;

    /// How far the statement in hand may run before it has to break, which is the tolerance until
    /// it has broken once and the width from then on.
    private int Tolerated => _hasBroken ? maxLineLength : maxLineLength + tolerance;

    private bool LineHasText => _currentLineLength > _lineIndentation * indentationSize;

    internal void Add(TextUnit unit)
    {
        if (unit.Indentation is { } indentation)
            AddStatement(unit.Text, indentation);
        else
            AddWord(unit.Text);
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

    internal StringBuilder AddText(string? text)
    {
        if (text is not null)
            AddPart(TextGroup.Parse(text));
        return _sb;
    }

    private void AddStatement(string text, int indentation)
    {
        _hasBroken = false;
        _statementIndentation = indentation;
        StartLine(indentation);
        AddText(text);
    }

    /// A part that broke ends its line, so the next one starts a line of its own.
    private void AddWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return;

        if (_partBroke)
            Continue(ContinuationIndentation);
        AddText($" {word}");
    }

    /// A part moves whole to a line of its own when it fits there and not on the line in progress;
    /// whether it broke is counted from there.
    private void AddPart(TextGroup part)
    {
        if (!Fits(part.Width) && LineHasText && FitsOnOwnLine(part.Flat, ContinuationIndentation))
            Continue(ContinuationIndentation);
        var breaks = _breaks;
        Place(part, trailing: 0);
        _partBroke = _breaks > breaks;
    }

    /// A group stays whole if it fits on the line together with what follows it up to the next
    /// break, <paramref name="trailing"/>.
    private void Place(TextGroup group, int trailing)
    {
        if (Fits(group.Width + trailing))
            AddRun(group.Flat);
        else
            Break(group, trailing);
    }

    private void Break(TextGroup group, int trailing)
    {
        var opened = _lineIndentation;
        for (var index = 0; index < group.Pieces.Count; index++)
        {
            switch (group.Pieces[index])
            {
                case TextRun run:
                    AddRun(run.Flat);
                    break;
                case TextBreak point:
                    Continue(BreakIndentation(point, opened));
                    break;
                case TextGroup nested:
                    Place(nested, group.TrailingAfter(index, trailing));
                    break;
            }
        }
    }

    private int BreakIndentation(TextBreak point, int opened)
        => Math.Max(ContinuationIndentation, point.Aligns ? opened : opened + 1);

    /// Text without a break point of its own. Where it does not fit, it moves whole to a line of its
    /// own if it fits there, and breaks at whitespace, a punctuation cue or mid-word if it does not.
    private void AddRun(string text)
    {
        if (!LineHasText)
            text = text.TrimStart();
        if (Fits(text.Length))
            Append(text);
        else if (LineHasText && FitsOnOwnLine(text, RunIndentation))
            ContinueWith(text);
        else
            BreakRun(text);
    }

    /// Where text breaking without a break point continues: on the step of the line it breaks.
    private int RunIndentation => Math.Max(ContinuationIndentation, _lineIndentation);

    private void BreakRun(string text)
    {
        var (first, rest) = BreakLine(text);
        Append(first);
        if (rest.Length > 0)
            ContinueWith(rest);
    }

    private void ContinueWith(string text)
    {
        Continue(RunIndentation);
        AddRun(text);
    }

    /// Continues the statement on a line of its own — which spends its tolerance: what is already
    /// costing a reader a seam gains nothing from lines wider than the page.
    private void Continue(int indentation)
    {
        _hasBroken = true;
        _breaks++;
        TrimLineEnd();
        StartLine(indentation);
    }

    private void StartLine(int indentation)
    {
        _lineIndentation = indentation;
        _sb.Append(Environment.NewLine);
        _sb.Append(new string(' ', _currentLineLength = indentation * indentationSize));
    }

    private void TrimLineEnd()
    {
        while (_sb.Length > 0 && _sb[_sb.Length - 1] == ' ')
            _sb.Length--;
    }

    private void Append(string text)
    {
        _sb.Append(text);
        _currentLineLength += text.Length;
    }

    private bool Fits(int width) => _currentLineLength + width <= Tolerated;

    /// Whether the whole text would fit on a line of its own. When it would, moving it there beats
    /// breaking it: what arrives as one piece is one expression, and a break inside it orphans part
    /// of it — where an expression that fits nowhere has to be broken regardless.
    private bool FitsOnOwnLine(string text, int indentation)
        => text.Trim().Length + indentation * indentationSize <= maxLineLength;

    private (string first, string rest) BreakLine(string text)
    {
        // Never the tolerance: a line that breaks is written to the width, and a line the tolerance
        // already carried past the width has no window left at all — where a line with nothing on
        // it yet takes a character regardless, or it would never be written.
        var window = Math.Max(maxLineLength - _currentLineLength, LineHasText ? 0 : 1);
        var cut = WhitespaceCut(text, window) ?? CueCut(text, window);
        if (cut is { } at)
            return (text[..at].TrimEnd(), text[at..].Trim());

        var first = UnbreakableStart(text[..Math.Min(window, text.Length)]);
        return (first, text[first.Length..].Trim());
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

    /// A segment without break position stays on the line (breaking mid-word), unless the line is
    /// already more than half used — then everything moves to the continuation line. A line with
    /// nothing on it has nowhere better to move it to.
    private string UnbreakableStart(string segment)
        => LineHasText && segment.Length < maxLineLength / 2 ? string.Empty : segment;

    private static bool IsFalseLineBreak(char c, char next)
        => c == '.' && (next == '.' || char.IsDigit(next));
}
