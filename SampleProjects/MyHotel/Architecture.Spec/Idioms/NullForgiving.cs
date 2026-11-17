using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 6d as an invariant over the syntax trees: a null-forgiving `!` marks a
/// shape not fixed yet (2b), and none stands on an expression that already holds one - not
/// `request!.Room!.RoomNumber`. The arguments of one call are expressions of their own, and so is a lambda.
/// One fact per project (Sources).</summary>
public class NullForgiving : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AnExpressionHoldsAtMostOneNullForgivingOperator(string project) =>
        Source.Projects[project].Nodes<PostfixUnaryExpressionSyntax>()
            .Has().None(DoublyForgiven);

    /// <summary>A `!` on an expression that already holds one.</summary>
    private static bool DoublyForgiven(SourceNode<PostfixUnaryExpressionSyntax> expression) =>
        IsForgiving(expression.Syntax) && Forgiving(WithinTheExpression(expression.Syntax.Operand)).Any();

    /// <summary>The operand and what it holds, short of an argument or a lambda.</summary>
    private static IEnumerable<SyntaxNode> WithinTheExpression(ExpressionSyntax operand) =>
        operand.DescendantNodesAndSelf(OfTheSameExpression);

    private static bool OfTheSameExpression(SyntaxNode node) =>
        node is not (ArgumentSyntax or AnonymousFunctionExpressionSyntax);

    private static IEnumerable<PostfixUnaryExpressionSyntax> Forgiving(IEnumerable<SyntaxNode> nodes) =>
        nodes.OfType<PostfixUnaryExpressionSyntax>().Where(IsForgiving);

    private static bool IsForgiving(PostfixUnaryExpressionSyntax expression) =>
        expression.IsKind(SyntaxKind.SuppressNullableWarningExpression);
}
