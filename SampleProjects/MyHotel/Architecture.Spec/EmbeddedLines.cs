using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;

namespace MyHotel.Architecture.Spec;

/// <summary>The lines inside a multi-line raw string literal, counted from 0 as the file's Lines are: an
/// embedded document or query keeps its own shape (CODING-STANDARD-BACKEND.md 0e), so its lines are not ours
/// to measure - neither their width (11a) nor their indentation (11c).</summary>
internal static class EmbeddedLines
{
    public static IReadOnlySet<int> Of(SourceFile file)
    {
        HashSet<int> lines = [.. RawStrings(file.Root).SelectMany(literal => Inside(file.SpanOf(literal)))];
        return lines;
    }

    /// <summary>The lines after the opening delimiter, the closing one included: the content and what sets
    /// its shape. A span counts lines from 1, so the line it starts on is the index of the line after
    /// it.</summary>
    private static IEnumerable<int> Inside(SourceSpan span) =>
        Enumerable.Range(span.Start.Line, span.End.Line - span.Start.Line);

    private static IEnumerable<SyntaxNodeOrToken> RawStrings(SyntaxNode root) =>
        root.DescendantTokens().Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
            .Select(token => (SyntaxNodeOrToken)token)
            .Concat(root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>()
                .Where(raw => raw.StringStartToken.IsKind(
                    SyntaxKind.InterpolatedMultiLineRawStringStartToken))
                .Select(raw => (SyntaxNodeOrToken)raw));
}
