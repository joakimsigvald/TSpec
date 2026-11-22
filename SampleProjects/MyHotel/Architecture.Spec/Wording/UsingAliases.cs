using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 1h as an invariant over the syntax trees: a using alias is longer than
/// a letter. Whether the word says what it stands for is review's.</summary>
public class UsingAliases : TSpec.Spec
{
    private const int ShortestWord = 2;

    [Fact]
    public void AUsingAliasIsAWordNotALetter() =>
        Source.Projects.Has().All(project => project.Nodes<UsingDirectiveSyntax>().Has().None(IsALetter));

    private static bool IsALetter(SourceNode<UsingDirectiveSyntax> directive) =>
        directive.Syntax.Alias?.Name.Identifier.Text.Length < ShortestWord;
}
