using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Builds the specification text, handling line-wrapping, indentation and capitalization
/// </summary>
internal class TextBuilder(int maxLineLength = 80, int indentationSize = 2)
{
    private const int _wrapIndentation = 3;
    private static readonly char[] _breakAfterCues = ['.', '(', '[', '{'];
    private readonly StringBuilder _sb = new();
    private int _currentLineLength;

    internal void AddPhraseOrSentence(string phrase)
    {
        if (char.IsUpper(phrase[0]))
            AddSentence(phrase);
        else
            AddPhrase(phrase);
    }

    internal void AddSentence(string phrase) => AddLine(phrase.Capitalize(), 0);

    internal void AddPhrase(string phrase, int indentation = 1) => AddLine(phrase, indentation);

    internal void AddWord(string word, string binder = " ")
    {
        if (!string.IsNullOrEmpty(word))
            AddText($"{binder}{word}");
    }

    internal StringBuilder AddText(string? text)
    {
        if (text is null)
            return _sb;

        var (first, rest) = IsExceedingMaxLineLength(text) ? BreakLine(text) : (text, null);
        _sb.Append(first);
        _currentLineLength += first.Length;
        if (rest is not null)
            AddLine(rest, _wrapIndentation);
        return _sb;
    }

    /// <summary>
    /// Get the built text
    /// </summary>
    /// <returns>The built text, trimmed and capitalized</returns>
    public override string ToString() => _sb.ToString().Trim().Capitalize();

    private void AddLine(string line, int indentation)
    {
        _sb.Append(Environment.NewLine);
        _sb.Append(new string(' ', _currentLineLength = indentation * indentationSize));
        AddText(line);
    }

    private bool IsExceedingMaxLineLength(string text)
        => text.Length + _currentLineLength > maxLineLength;

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
            char next = i + 1 < segment.Length ? segment[i + 1] : ' ';
            if (!IsLineBreakPossibleAfter(segment[i], next))
                continue;

            var start = segment[..(i + 1)].TrimEnd();
            return start.Length > 0 ? start : null;
        }
        return null;
    }

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
