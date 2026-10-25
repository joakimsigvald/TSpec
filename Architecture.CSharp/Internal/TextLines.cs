using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The lines of text in a file, counted from 0: those after the first line of a string or a comment that spans
/// lines, each with the line of code its text opens on.
/// </summary>
internal static class TextLines
{
    internal static IReadOnlyDictionary<int, int> Of(SyntaxNode root)
    {
        Dictionary<int, int> openings = [];
        foreach (var text in Texts(root).Select(span => root.SyntaxTree.GetLineSpan(span)).OrderBy(Start))
            Open(openings, text);
        return openings;
    }

    // A string in an interpolation of another is text of the outer one, which opens first.
    private static void Open(Dictionary<int, int> openings, FileLinePositionSpan text)
    {
        var opening = openings.GetValueOrDefault(Start(text), Start(text));
        foreach (var line in Enumerable.Range(Start(text) + 1, text.EndLinePosition.Line - Start(text)))
            openings.TryAdd(line, opening);
    }

    private static IEnumerable<TextSpan> Texts(SyntaxNode root)
        => root.DescendantNodes().Where(IsString).Select(node => node.Span)
            .Concat(root.DescendantTrivia().Where(IsBlockComment).Select(comment => comment.Span));

    private static bool IsString(SyntaxNode node) => node is LiteralExpressionSyntax or InterpolatedStringExpressionSyntax;

    private static bool IsBlockComment(SyntaxTrivia trivia)
        => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);

    private static int Start(FileLinePositionSpan text) => text.StartLinePosition.Line;
}
