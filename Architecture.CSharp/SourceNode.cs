using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A node of a source file's syntax, with the file it is in
/// </summary>
/// <typeparam name="T">The kind of node</typeparam>
public sealed class SourceNode<T> where T : SyntaxNode
{
    internal SourceNode(SourceFile file, T syntax)
    {
        File = file;
        Syntax = syntax;
    }

    /// <summary>
    /// The file the node is in
    /// </summary>
    public SourceFile File { get; }

    /// <summary>
    /// The node's syntax
    /// </summary>
    public T Syntax { get; }

    /// <summary>
    /// Where the node starts and ends
    /// </summary>
    public SourceSpan Span => File.SpanOf(Syntax);

    /// <summary>
    /// The node as a failure names it: its file and the line it starts on
    /// </summary>
    public override string ToString() => $"{File}:{Span.Start.Line}";
}
