using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 1h as an invariant over the syntax trees: a using alias is longer than
/// a letter. Whether the word says what it stands for is review's. One fact per project (Sources).</summary>
public class UsingAliases : TSpec.Spec
{
    private const int ShortestWord = 2;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AUsingAliasIsAWordNotALetter(string project) =>
        Source.Projects[project].Nodes<UsingDirectiveSyntax>().Has().None(IsALetter);

    private static bool IsALetter(SourceNode<UsingDirectiveSyntax> directive) =>
        directive.Syntax.Alias?.Name.Identifier.Text.Length < ShortestWord;
}
