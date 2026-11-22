using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11c as an invariant over the syntax trees, beside NestingDepth's
/// ceiling: a level is a step of the structure. A line is at most one level deeper than the line before it,
/// one more for each construct that line leaves open; a brace on a line of its own stands at the level of the
/// line its construct opens on, and a closing brace or bracket at the level of its opening one.</summary>
public class Indentation : TSpec.Spec
{
    [Fact]
    public void ALineIsAtMostALevelDeeperThanTheLineBeforeIt() =>
        Source.Projects.Has().All(project => project.Files.SelectMany(OversteppingLines).Is().Empty());

    [Fact]
    public void ABraceOnALineOfItsOwnStandsAtTheLevelOfItsConstruct() =>
        Source.Projects.Has().All(project => project.Files.SelectMany(MisbracedLines).Is().Empty());

    /// <summary>Every line indented deeper than the line before it allows: one level, and one more for each
    /// construct that line leaves open.</summary>
    private static IEnumerable<SourceLine> OversteppingLines(SourceFile file)
    {
        var opens = Opens(file);
        var lines = Counted(file);
        return lines.Zip(lines.Skip(1))
            .Where(pair => Step(pair.First, pair.Second) > Allowed(opens, pair.First))
            .Select(pair => pair.Second);
    }

    private static int Step(SourceLine before, SourceLine line) =>
        Indents.Of(line.Text) - Indents.Of(before.Text);

    private static int Allowed(IReadOnlyDictionary<int, int> opens, SourceLine line) =>
        Indents.Width * Math.Max(1, opens.GetValueOrDefault(line.Number));

    /// <summary>How many constructs each line leaves open, by its number: its opening brackets less its
    /// closing ones.</summary>
    private static IReadOnlyDictionary<int, int> Opens(SourceFile file) =>
        file.Root.DescendantTokens()
            .Where(token => Sign(token) != 0)
            .GroupBy(token => file.Line(token))
            .ToDictionary(line => line.Key, line => line.Sum(Sign));

    private static int Sign(SyntaxToken token) =>
        token.Kind() switch
        {
            SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken or SyntaxKind.OpenBraceToken => 1,
            SyntaxKind.CloseParenToken or SyntaxKind.CloseBracketToken or SyntaxKind.CloseBraceToken => -1,
            _ => 0,
        };

    /// <summary>The lines a reader steps through: code, not blank and not a directive. The text of an
    /// embedded document or a block comment is prose, aligned as prose.</summary>
    private static IReadOnlyList<SourceLine> Counted(SourceFile file) =>
        [.. file.SourceLines.Where(line => line.IsCode && IsAStep(line.Text))];

    private static bool IsAStep(string text) =>
        text.TrimStart() is { Length: > 0 } code && !code.StartsWith('#');

    /// <summary>"file:line" for every brace that starts its line off the level it answers to.</summary>
    private static IEnumerable<string> MisbracedLines(SourceFile file) =>
        file.Root.DescendantTokens()
            .Where(token => AnchorOf(token) is not null && StartsItsLine(file, token))
            .Where(token => file.SourceLines[file.Line(token) - 1].IsCode)
            .Where(token => file.Column(token) != IndentOf(file, AnchorOf(token)!.Value))
            .Select(token => $"{file}:{file.Line(token)}");

    /// <summary>The token whose line a brace answers to: its construct's head for an opening brace, its
    /// opening partner for a closing one.</summary>
    private static SyntaxToken? AnchorOf(SyntaxToken token) =>
        token.Kind() switch
        {
            SyntaxKind.OpenBraceToken => HeadOf(token),
            SyntaxKind.CloseBraceToken or SyntaxKind.CloseBracketToken => OpeningOf(token),
            _ => null,
        };

    /// <summary>The line a construct opens on; a bare block and an element initializer have none of their
    /// own.</summary>
    private static SyntaxToken? HeadOf(SyntaxToken brace) =>
        brace.Parent switch
        {
            BlockSyntax { Parent: BlockSyntax } => null,
            BlockSyntax { Parent: { } owner } => FirstOf(owner),
            InitializerExpressionSyntax { Parent: InitializerExpressionSyntax } => null,
            InitializerExpressionSyntax { Parent: { } owner } => FirstOf(owner),
            AccessorListSyntax { Parent: { } owner } => FirstOf(owner),
            BaseTypeDeclarationSyntax type => FirstOf(type),
            SwitchExpressionSyntax switched => switched.GetFirstToken(),
            SwitchStatementSyntax switched => switched.SwitchKeyword,
            AnonymousObjectCreationExpressionSyntax created => created.NewKeyword,
            _ => null,
        };

    /// <summary>A closing brace's opening one, or a collection expression's opening bracket; an
    /// interpolation's braces are text.</summary>
    private static SyntaxToken? OpeningOf(SyntaxToken closing) =>
        closing.Parent switch
        {
            InterpolationSyntax => null,
            CollectionExpressionSyntax collection => collection.OpenBracketToken,
            { } owner when closing.IsKind(SyntaxKind.CloseBraceToken) =>
                OpenBraceOf(owner),
            _ => null,
        };

    private static SyntaxToken? OpenBraceOf(SyntaxNode owner) =>
        owner.ChildTokens()
            .Where(token => token.IsKind(SyntaxKind.OpenBraceToken))
            .Select(token => (SyntaxToken?)token)
            .FirstOrDefault();

    /// <summary>A declaration's first token after its attributes, which stand on lines of their
    /// own.</summary>
    private static SyntaxToken FirstOf(SyntaxNode node) =>
        TokenOf(node.ChildNodesAndTokens().First(child => !child.IsKind(SyntaxKind.AttributeList)));

    private static SyntaxToken TokenOf(SyntaxNodeOrToken child) =>
        child.AsNode() is { } node ? node.GetFirstToken() : child.AsToken();

    private static bool StartsItsLine(SourceFile file, SyntaxToken token) =>
        file.Line(token.GetPreviousToken()) < file.Line(token);

    private static int IndentOf(SourceFile file, SyntaxToken token) =>
        Indents.Of(file.SourceLines[file.Line(token) - 1].Text);
}
