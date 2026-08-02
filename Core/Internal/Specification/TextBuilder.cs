using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Lays composed text out: line-wrapping and indentation, at the width it is given. The last phase,
/// and the only one that decides where a line ends — which is why it must be handed text nothing
/// will shorten or lengthen afterwards.
/// </summary>
internal class TextBuilder(int maxLineLength = TextBuilder.PageWidth, int indentationSize = 2)
{
    /// How wide specification text is written when nothing indents it.
    internal const int PageWidth = 80;

    private const int WrapIndentation = 3;
    private static readonly char[] _breakAfterCues = ['.', '(', '[', '{'];
    private readonly StringBuilder _sb = new();
    private int _currentLineLength;

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
        if (text is null)
            return _sb;

        if (IsExceedingMaxLineLength(text) && FitsOnOwnLine(text) && _currentLineLength > 0)
        {
            AddLine(text.Trim(), WrapIndentation);
            return _sb;
        }

        var (first, rest) = IsExceedingMaxLineLength(text) ? BreakLine(text) : (text, null);
        _sb.Append(first);
        _currentLineLength += first.Length;
        if (rest is not null)
            AddLine(rest, WrapIndentation);
        return _sb;
    }

    private void AddLine(string line, int indentation)
    {
        _sb.Append(Environment.NewLine);
        _sb.Append(new string(' ', _currentLineLength = indentation * indentationSize));
        AddText(line);
    }

    private bool IsExceedingMaxLineLength(string text)
        => text.Length + _currentLineLength > maxLineLength;

    /// Whether the whole text would fit on a continuation line of its own. When it would, moving it
    /// there beats breaking it: what arrives as one piece is one expression, and a break inside it
    /// orphans part of it — where an expression that fits nowhere has to be broken regardless.
    private bool FitsOnOwnLine(string text)
        => text.Trim().Length + WrapIndentation * indentationSize <= maxLineLength;

    private (string first, string? rest) BreakLine(string text)
    {
        var fitInLine = text[..(maxLineLength - _currentLineLength)];
        var first = BreakableStart(fitInLine) ?? UnbreakableStart(fitInLine);
        return (first, text[first.Length..].Trim());
    }

    private static string? BreakableStart(string segment)
    {
        for (int i = segment.Length - 1; i >= 0; i--)
        {
            if (!IsLineBreakPossibleAfter(segment[i], Next(segment, i)))
                continue;

            var start = segment[..(i + 1)].TrimEnd();
            return start.Length > 0 ? start : null;
        }
        return null;
    }

    private static char Next(string segment, int i) => i + 1 < segment.Length ? segment[i + 1] : ' ';

    /// A segment without break position stays on the line (breaking mid-word),
    /// unless the line is already more than half used — then everything moves
    /// to the continuation line.
    private string UnbreakableStart(string segment)
        => segment.Length < maxLineLength / 2 ? string.Empty : segment;

    private static bool IsLineBreakPossibleAfter(char c, char next)
        => char.IsWhiteSpace(c)
        || _breakAfterCues.Contains(c) && !IsFalseLineBreak(c, next);

    private static bool IsFalseLineBreak(char c, char next)
        => c == '.' && (next == '.' || char.IsDigit(next));
}
