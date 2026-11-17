using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11d as an invariant over the syntax trees: an expression-bodied member
/// and a switch arm are whole on one line, or `=>` ends the line and the body starts on the next, one level
/// in from where the member starts (`=>` never opening a line is Arrows'). One fact per project
/// (Sources).</summary>
public class MemberBodies : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AMemberIsOnOneLineOrItsBodyStartsBelowOneLevelIn(string project) =>
        Source.Projects[project].Files.SelectMany(MisplacedMemberBodies).Is().Empty();

    /// <summary>"file:line" for every member or arm whose body is neither on its line nor below it, one
    /// level in.</summary>
    private static IEnumerable<string> MisplacedMemberBodies(SourceFile file) =>
        file.Root.DescendantNodes().Select(BodyOf).OfType<Body>()
            .Where(body => !Shaped(body, file))
            .Select(body => $"{file}:{file.Line(body.Arrow)}");

    /// <summary>Whole on one line, or the body alone below the arrow, one level in from the member's
    /// line.</summary>
    private static bool Shaped(Body body, SourceFile file) =>
        file.Line(Head(body.Member)) == file.End(body.Expression)
        || (file.Line(body.Expression) == file.Line(body.Arrow) + 1 && OneLevelIn(body, file));

    private static bool OneLevelIn(Body body, SourceFile file) =>
        file.Column(body.Expression)
        == Indents.Of(file.SourceLines[file.Line(Head(body.Member)) - 1].Text) + Indents.Width;

    private static Body? BodyOf(SyntaxNode node) =>
        node switch
        {
            ArrowExpressionClauseSyntax clause => new(clause.Parent!, clause.ArrowToken, clause.Expression),
            SwitchExpressionArmSyntax arm => new(arm, arm.EqualsGreaterThanToken, arm.Expression),
            _ => null,
        };

    /// <summary>Where the member starts on the page: its first token past its attributes.</summary>
    private static SyntaxToken Head(SyntaxNode member) =>
        member.DescendantTokens().First(token => !token.Parent!.AncestorsAndSelf().Any(IsAttributes));

    private static bool IsAttributes(SyntaxNode node) => node is AttributeListSyntax;

    /// <summary>An expression-bodied member or a switch arm, its arrow and its body.</summary>
    private sealed record Body(SyntaxNode Member, SyntaxToken Arrow, ExpressionSyntax Expression);
}
