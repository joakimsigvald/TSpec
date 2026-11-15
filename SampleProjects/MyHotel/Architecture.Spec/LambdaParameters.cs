using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 6c as an invariant over the syntax trees: every lambda's parameter is
/// a word of 2 to 12 letters, the first lowercase. `_` is a discard and is never used; only in a spec project
/// may a body start with `_`, the subject, and use it nowhere else (`_ => _.Load(..)`, `_ => _ with { .. }`).
/// One fact per project (Sources).</summary>
public class LambdaParameters : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void ALambdasParameterIsAWord(string project) =>
        Source.Projects[project].Nodes<ParameterSyntax>().Has().None(parameter => Unworded(parameter));

    /// <summary>A lambda's parameter that is not a word.</summary>
    private static bool Unworded(SourceNode<ParameterSyntax> parameter) =>
        LambdaOf(parameter.Syntax) is { } lambda
        && !IsNamed(parameter.Syntax, lambda, IsSpec(parameter.File));

    private static LambdaExpressionSyntax? LambdaOf(ParameterSyntax parameter) =>
        parameter.Parent switch
        {
            SimpleLambdaExpressionSyntax lambda => lambda,
            ParameterListSyntax { Parent: ParenthesizedLambdaExpressionSyntax lambda } => lambda,
            _ => null,
        };

    private static bool IsNamed(ParameterSyntax parameter, LambdaExpressionSyntax lambda, bool spec) =>
        parameter.Identifier.ValueText is "_"
            ? IsDiscardOrReceiver(lambda, spec)
            : IsWord(parameter.Identifier.ValueText);

    /// <summary>2 to 12 letters, the first lowercase.</summary>
    private static bool IsWord(string name) =>
        name.Length is >= 2 and <= 12
        && char.IsAsciiLetterLower(name[0])
        && name.All(char.IsAsciiLetter);

    /// <summary>Unused, or in a spec project the subject the body starts with and nothing more.</summary>
    private static bool IsDiscardOrReceiver(LambdaExpressionSyntax lambda, bool spec) =>
        UsesOfDiscard(lambda) switch
        {
            [] => true,
            [var only] => spec && only.Identifier == lambda.Body.GetFirstToken(),
            _ => false,
        };

    /// <summary>The body's references to `_`, short of a nested lambda with a `_` of its own.</summary>
    private static IReadOnlyList<IdentifierNameSyntax> UsesOfDiscard(LambdaExpressionSyntax lambda) =>
        [.. lambda.Body.DescendantNodesAndSelf(node => !DeclaresDiscard(node))
            .OfType<IdentifierNameSyntax>()
            .Where(name => name.Identifier.ValueText is "_")];

    private static bool DeclaresDiscard(SyntaxNode node) =>
        node is LambdaExpressionSyntax nested
        && ParametersOf(nested).Any(parameter => parameter.Identifier.ValueText is "_");

    private static IEnumerable<ParameterSyntax> ParametersOf(LambdaExpressionSyntax lambda) =>
        lambda switch
        {
            SimpleLambdaExpressionSyntax simple => [simple.Parameter],
            ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters,
            _ => [],
        };

    private static bool IsSpec(SourceFile file) => Source.Projects[file.Project].IsSpecProject;
}
