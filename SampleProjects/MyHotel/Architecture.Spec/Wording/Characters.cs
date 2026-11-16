using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 1a as an invariant over the syntax trees: code and comments are pure
/// ASCII. A literal is content, not code - a user's text - so a string or character literal may hold any
/// character; an interpolation's holes are code. One fact per project (Sources).</summary>
public class Characters : TSpec.Spec
{
    private static readonly SyntaxKind[] LiteralKinds =
    [
        SyntaxKind.StringLiteralToken,
        SyntaxKind.CharacterLiteralToken,
        SyntaxKind.SingleLineRawStringLiteralToken,
        SyntaxKind.MultiLineRawStringLiteralToken,
        SyntaxKind.InterpolatedStringTextToken,
        SyntaxKind.Utf8StringLiteralToken,
        SyntaxKind.Utf8SingleLineRawStringLiteralToken,
        SyntaxKind.Utf8MultiLineRawStringLiteralToken,
    ];

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void CodeAndCommentsAreAscii(string project) =>
        Source.Projects[project].Files.SelectMany(NonAsciiLines).Is().Empty();

    /// <summary>"file:line" for every line with a character beyond ASCII outside every literal.</summary>
    private static IEnumerable<string> NonAsciiLines(SourceFile file)
    {
        var text = file.Root.SyntaxTree.GetText();
        return NonAsciiIn(text, Literals(file.Root))
            .Select(position => text.Lines.GetLinePosition(position).Line + 1)
            .Distinct()
            .Select(line => $"{file}:{line}");
    }

    private static IEnumerable<int> NonAsciiIn(SourceText text, IReadOnlyList<TextSpan> literals) =>
        Enumerable.Range(0, text.Length)
            .Where(position => !char.IsAscii(text[position]))
            .Where(position => !literals.Any(literal => literal.Contains(position)));

    private static IReadOnlyList<TextSpan> Literals(SyntaxNode root) =>
        [.. root.DescendantTokens()
            .Where(token => LiteralKinds.Contains(token.Kind()))
            .Select(token => token.Span)];
}
