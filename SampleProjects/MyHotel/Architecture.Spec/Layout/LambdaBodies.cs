using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11e as an invariant over the syntax trees: a lambda's expression body
/// starts on the arrow's line, and a block body opens on the line after it (the arrow on the parameters' line
/// is Arrows').</summary>
public class LambdaBodies : TSpec.Spec
{
    [Fact]
    public void AnExpressionBodyStartsOnTheArrowsLineAndABlockOnTheNext() =>
        Source.Projects.Has().All(project => project.Files.SelectMany(MisplacedLambdaBodies).Is().Empty());

    /// <summary>"file:line" for every lambda whose body opens elsewhere than its rule says.</summary>
    private static IEnumerable<string> MisplacedLambdaBodies(SourceFile file) =>
        file.Root.DescendantNodes().OfType<LambdaExpressionSyntax>()
            .Where(lambda => file.Line(lambda.Body) != file.Line(lambda.ArrowToken) + Below(lambda.Body))
            .Select(lambda => $"{file}:{file.Line(lambda.ArrowToken)}");

    /// <summary>How many lines below its arrow a body opens: a block on the next, an expression on the
    /// arrow's own.</summary>
    private static int Below(CSharpSyntaxNode body) => body is BlockSyntax ? 1 : 0;
}
