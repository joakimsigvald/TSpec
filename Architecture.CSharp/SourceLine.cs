namespace TSpec.Architecture.CSharp;

/// <summary>
/// A line of a source file, with the file it is in
/// </summary>
public sealed class SourceLine
{
    internal SourceLine(SourceFile file, int number, string text, bool isCode)
    {
        File = file;
        Number = number;
        Text = text;
        IsCode = isCode;
    }

    /// <summary>
    /// The file the line is in
    /// </summary>
    public SourceFile File { get; }

    /// <summary>
    /// The line's number, counted from 1 as an editor counts it
    /// </summary>
    public int Number { get; }

    /// <summary>
    /// The line as it is written, without its line break
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Whether the line is code: false when it continues a string or a comment that opens on an earlier line,
    /// such as the content of a raw string literal, whose text is no code of the file's
    /// </summary>
    public bool IsCode { get; }

    /// <summary>
    /// The line as a failure names it: its file and its number
    /// </summary>
    public override string ToString() => $"{File}:{Number}";
}
