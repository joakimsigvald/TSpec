using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11g as an invariant over the syntax trees: exactly one empty line
/// stands between two members of a type, except between two single-line members of one kind, which stand
/// together. A member runs from its first comment to its end, its attributes included, and a constant is a
/// kind of its own, as C# names it. One fact per project (Sources); a breach is counted.</summary>
public class MemberSpacing : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void MembersAreOneEmptyLineApartUnlessTwoSingleLineOnesOfAKind(string project) =>
        Breach(project).Is().Null();

    [Theory]
    [InlineData("class C\n{\n    int a;\n    int b;\n}")]
    [InlineData("class C\n{\n    int A() => 1;\n    int B() => 2;\n}")]
    [InlineData("class C\n{\n    const int A = 1;\n\n    int b;\n}")]
    [InlineData("class C\n{\n    /// <summary>A.</summary>\n    int a;\n\n    int b;\n}")]
    [InlineData("class C\n{\n    [Fact]\n    void A() { }\n\n    void B() { }\n}")]
    public void TheseMembersAreSpacedAs11gSays(string declaration) =>
        Misspaced(TypeOf(declaration)).Is().Empty();

    [Theory]
    [InlineData("class C\n{\n    int a;\n\n    int b;\n}")]
    [InlineData("class C\n{\n    const int A = 1;\n    int b;\n}")]
    [InlineData("class C\n{\n    /// <summary>A.</summary>\n    int a;\n    int b;\n}")]
    [InlineData("class C\n{\n    [Fact]\n    void A() { }\n    void B() { }\n}")]
    [InlineData("class C\n{\n    void A() { }\n\n\n    void B() { }\n}")]
    public void TheseAreNot(string declaration) =>
        Misspaced(TypeOf(declaration)).Has().OneItem();

    private static string? Breach(string project) =>
        Misspaced(project) is [var first, ..] and var misspaced
            ? $"{misspaced.Count} members have the wrong empty lines above them, the first at {first}"
            : null;

    /// <summary>"file:line" for each member of the project whose distance to the member above breaks
    /// 11g.</summary>
    private static IReadOnlyList<string> Misspaced(string project) =>
        [.. Source.Projects[project].Files.SelectMany(MisspacedIn)];

    private static IEnumerable<string> MisspacedIn(SourceFile file) =>
        file.Root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .SelectMany(Misspaced)
            .Select(member => $"{file}:{file.Line(member)}");

    /// <summary>The members of a type with the wrong number of empty lines above them.</summary>
    private static IEnumerable<MemberDeclarationSyntax> Misspaced(TypeDeclarationSyntax type) =>
        type.Members.Zip(type.Members.Skip(1))
            .Where(pair => EmptyLinesBetween(pair.First, pair.Second) != Spacing(pair.First, pair.Second))
            .Select(pair => pair.Second);

    private static int Spacing(MemberDeclarationSyntax above, MemberDeclarationSyntax below) =>
        IsSingleLine(above) && IsSingleLine(below) && IsOfOneKind(above, below) ? 0 : 1;

    private static int EmptyLinesBetween(MemberDeclarationSyntax above, MemberDeclarationSyntax below) =>
        LineOf(below, StartOf(below)) - LineOf(above, above.Span.End) - 1;

    private static bool IsSingleLine(MemberDeclarationSyntax member) =>
        LineOf(member, StartOf(member)) == LineOf(member, member.Span.End);

    private static bool IsOfOneKind(MemberDeclarationSyntax above, MemberDeclarationSyntax below) =>
        above.Kind() == below.Kind() && IsConstant(above) == IsConstant(below);

    private static bool IsConstant(MemberDeclarationSyntax member) =>
        member.Modifiers.Any(SyntaxKind.ConstKeyword);

    /// <summary>Where a member starts: at its first comment, or its first token when it has none.</summary>
    private static int StartOf(MemberDeclarationSyntax member) =>
        member.GetLeadingTrivia().FirstOrDefault(IsWritten) is { RawKind: not 0 } comment
            ? comment.SpanStart
            : member.SpanStart;

    private static bool IsWritten(SyntaxTrivia trivia) =>
        !trivia.IsKind(SyntaxKind.WhitespaceTrivia) && !trivia.IsKind(SyntaxKind.EndOfLineTrivia);

    private static int LineOf(SyntaxNode node, int position) =>
        node.SyntaxTree.GetText().Lines.GetLineFromPosition(position).LineNumber;

    /// <summary>The type a snippet declares.</summary>
    private static TypeDeclarationSyntax TypeOf(string declaration) =>
        (TypeDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(declaration).Members[0];
}
