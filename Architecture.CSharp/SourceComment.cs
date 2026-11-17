using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A comment of a source file, as a reader sees it: <c>//</c> lines in a row, each on its own line, are one
/// comment, as the lines of a <c>///</c> block are
/// </summary>
public sealed class SourceComment
{
    internal SourceComment(SourceFile file, CommentKind kind, SourceSpan span, string text, bool isTrailing, SyntaxNode? leads)
    {
        File = file;
        Kind = kind;
        Span = span;
        Text = text;
        IsTrailing = isTrailing;
        Leads = leads;
    }

    /// <summary>
    /// The file the comment is in
    /// </summary>
    public SourceFile File { get; }

    /// <summary>
    /// How the comment is written
    /// </summary>
    public CommentKind Kind { get; }

    /// <summary>
    /// Where the comment starts and ends; its <c>Lines</c> are the lines it covers
    /// </summary>
    public SourceSpan Span { get; }

    /// <summary>
    /// The comment as it is written, its markers included
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Whether the comment follows code on its line
    /// </summary>
    public bool IsTrailing { get; }

    /// <summary>
    /// The code the comment stands above: the largest declaration or statement that starts right after it,
    /// its attributes included. Null for a trailing comment and one at the end of the file
    /// </summary>
    public SyntaxNode? Leads { get; }

    /// <summary>
    /// The comment as a failure names it: its file and the line it starts on
    /// </summary>
    public override string ToString() => $"{File}:{Span.Start.Line}";
}
