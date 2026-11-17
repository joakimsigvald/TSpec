using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11d and 11e in part, as an invariant over the syntax trees: `=>` never
/// starts a line - neither a member's or switch arm's nor a lambda's. One fact per project (Sources); a
/// breach is counted. Where the body goes is MemberBodies' and LambdaBodies'.</summary>
public class Arrows : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AnArrowNeverStartsALine(string project) =>
        Source.Projects[project].Nodes<SyntaxNode>().Has().None(HoldsAnArrowStartingALine);

    /// <summary>Every token is a child of one node, so looking at each node's own tokens sees every
    /// arrow.</summary>
    private static bool HoldsAnArrowStartingALine(SourceNode<SyntaxNode> node) =>
        node.Syntax.ChildTokens()
            .Where(token => token.IsKind(SyntaxKind.EqualsGreaterThanToken))
            .Any(arrow => node.File.Line(arrow) > node.File.Line(arrow.GetPreviousToken()));
}
