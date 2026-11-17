using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A file's comments as a reader sees them: <c>//</c> lines in a row, each on its own line, are one comment, as
/// the lines of a <c>///</c> block are.
/// </summary>
internal static class FileComments
{
    internal static IReadOnlyList<SourceComment> Of(SourceFile file)
        => [.. Runs(file, file.Root.DescendantTrivia().Where(IsComment)).Select(run => CommentOf(file, run))];

    private static IEnumerable<IReadOnlyList<SyntaxTrivia>> Runs(SourceFile file, IEnumerable<SyntaxTrivia> comments)
    {
        List<SyntaxTrivia> run = [];
        foreach (var comment in comments)
        {
            if (run.Count > 0 && !Continues(file, run[^1], comment))
            {
                yield return run;
                run = [];
            }

            run.Add(comment);
        }

        if (run.Count > 0)
            yield return run;
    }

    // A note after code stays with its code, so only // lines that stand on their own join.
    private static bool Continues(SourceFile file, SyntaxTrivia last, SyntaxTrivia next)
        => IsALineOfItsOwn(last) && IsALineOfItsOwn(next) && LineOf(file, next) == LineOf(file, last) + 1;

    private static bool IsALineOfItsOwn(SyntaxTrivia comment)
        => KindOf(comment) is CommentKind.Line && !IsTrailing(comment);

    private static SourceComment CommentOf(SourceFile file, IReadOnlyList<SyntaxTrivia> run)
    {
        var span = TextSpan.FromBounds(run[0].SpanStart, EndOf(run[^1]));
        var isTrailing = IsTrailing(run[0]);
        return new(
            file,
            KindOf(run[0]),
            file.SpanOf(span),
            file.Root.SyntaxTree.GetText().ToString(span),
            isTrailing,
            isTrailing ? null : LeadsOf(run[^1].Token));
    }

    // A documentation comment ends with the line break of its last line, which is no part of it.
    private static int EndOf(SyntaxTrivia comment) => comment.SpanStart + comment.ToString().TrimEnd().Length;

    private static int LineOf(SourceFile file, SyntaxTrivia comment) => file.SpanOf(comment.Span).Start.Line;

    private static bool IsTrailing(SyntaxTrivia comment) => comment.Token.TrailingTrivia.Contains(comment);

    private static SyntaxNode? LeadsOf(SyntaxToken token)
        => token.Parent is { } node && Starts(node, token) ? Outermost(node, token) : null;

    private static SyntaxNode Outermost(SyntaxNode node, SyntaxToken token)
        => node.Parent is { } parent && Starts(parent, token) ? Outermost(parent, token) : node;

    // The file itself starts with its first token too, but a comment above that token leads what follows.
    private static bool Starts(SyntaxNode node, SyntaxToken token)
        => node is not CompilationUnitSyntax && node.GetFirstToken() == token;

    private static bool IsComment(SyntaxTrivia trivia) => KindOrNull(trivia) is not null;

    private static CommentKind KindOf(SyntaxTrivia comment) => KindOrNull(comment)!.Value;

    private static CommentKind? KindOrNull(SyntaxTrivia trivia) => trivia.Kind() switch
    {
        SyntaxKind.SingleLineCommentTrivia => CommentKind.Line,
        SyntaxKind.MultiLineCommentTrivia => CommentKind.Block,
        SyntaxKind.SingleLineDocumentationCommentTrivia => CommentKind.Documentation,
        SyntaxKind.MultiLineDocumentationCommentTrivia => CommentKind.DocumentationBlock,
        _ => null,
    };
}
