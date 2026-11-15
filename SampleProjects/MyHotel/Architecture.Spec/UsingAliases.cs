using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 1h as an invariant over the syntax trees: a using alias is longer than
/// a letter. Whether the word says what it stands for is review's. One fact per project (Sources).</summary>
public class UsingAliases : TSpec.Spec
{
    private const int ShortestWord = 2;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AUsingAliasIsAWordNotALetter(string project) =>
        Breach(project).Is().Null();

    private static string? Breach(string project) =>
        Sites(project) is [var first, ..] and var sites
            ? $"{sites.Count} using aliases are a single letter, the first at {first}"
            : null;

    /// <summary>"file:line" for every alias of the project shorter than a word.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Source.Projects[project].Nodes<UsingDirectiveSyntax>().Where(IsALetter).Select(Reported)];

    private static bool IsALetter(SourceNode<UsingDirectiveSyntax> directive) =>
        directive.Syntax.Alias?.Name.Identifier.Text.Length < ShortestWord;

    private static string Reported(SourceNode<UsingDirectiveSyntax> directive) =>
        $"{directive.File}:{directive.File.Line(directive.Syntax)}";
}
