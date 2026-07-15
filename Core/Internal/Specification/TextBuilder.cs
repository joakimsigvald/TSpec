using System.Text;

namespace TSpec.Internal.Specification;

/// <summary>
/// Builds the specification text, handling line-wrapping, indentation and capitalization
/// </summary>
internal class TextBuilder(int maxLineLength = 80, int indentationSize = 2)
{
    private const int _maxIndentation = 3;
    private readonly StringBuilder _sb = new();
    private int _currentLineLength;

    internal void AddPhraseOrSentence(string phrase)
    {
        if (char.IsUpper(phrase[0]))
            AddSentence(phrase);
        else AddPhrase(phrase);
    }

    internal void AddPhrase(string phrase, int indentation = 1)
        => AddIndentedLine(phrase, indentation);

    internal void AddSentence(string phrase)
        => AddCapitalizedLine(phrase);

    internal void AddWord(string word, string binder = " ")
    {
        if (string.IsNullOrEmpty(word))
            return;
        AddText($"{binder}{word}");
    }

    internal StringBuilder AddCapitalizedLine(string line) => AddIndentedLine(line.Capitalize(), 0);

    internal StringBuilder AddIndentedLine(string line, int indentation)
    {
        _sb.Append(Environment.NewLine);
        _sb.Append(new string(' ', _currentLineLength = indentation * indentationSize));
        AddText(line);
        return _sb;
    }

    internal StringBuilder AddText(string? text)
    {
        if (text is null)
            return _sb;
        var (first, rest) = IsExceedingMaxLineLength(text) ? BreakLine(text) : (text, null);
        _sb.Append(first);
        _currentLineLength += first.Length;
        return rest is null ? _sb : AddIndentedLine(rest, _maxIndentation);
    }

    private bool IsExceedingMaxLineLength(string text)
        => text.Length + _currentLineLength > maxLineLength;

    private (string first, string rest) BreakLine(string text)
    {
        var fitInLine = text[..(maxLineLength - _currentLineLength)];
        var nextChar = ' ';
        char[] chars = [..fitInLine
            .Reverse()
            .SkipWhile(c =>
            {
                var possibleLineBreak = IsLineBreakPossibleAfter(c, nextChar);
                nextChar = c;
                return !possibleLineBreak;
            })
            .Reverse()];
        var first = new string(chars).TrimEnd();
        if (string.IsNullOrEmpty(first))
            first = fitInLine.Length < maxLineLength / 2 ? string.Empty : fitInLine;
        var rest = text[first.Length..].Trim();
        return (first, rest);
    }

    /// <summary>
    /// Get the built text
    /// </summary>
    /// <returns>The built text, trimmed and capitalized</returns>
    public override string ToString() => _sb.ToString().Trim().Capitalize();

    private static readonly char[] _lineBreakCues = ['.', '(', '[', '{'];

    private static bool IsLineBreakPossibleAfter(char c, char next)
        => char.IsWhiteSpace(c)
        || _lineBreakCues.Contains(c) && !IsFalseLineBreak(c, next);

    private static bool IsFalseLineBreak(char c, char next)
        => c == '.' && (next == '.' || char.IsDigit(next));
}