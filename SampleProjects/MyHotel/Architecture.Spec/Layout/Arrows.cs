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
        Breach(project).Is().Null();

    private static string? Breach(string project) =>
        Leading(project) is [var first, ..] and var leading
            ? $"{leading.Count} arrows start a line, the first at {first}"
            : null;

    /// <summary>"file:line" for every arrow in the project that opens its line.</summary>
    private static IReadOnlyList<string> Leading(string project) =>
        [.. Source.Projects[project].Files.SelectMany(LeadingIn)];

    private static IEnumerable<string> LeadingIn(SourceFile file) =>
        file.Root.DescendantTokens()
            .Where(token => token.IsKind(SyntaxKind.EqualsGreaterThanToken))
            .Where(arrow => file.Line(arrow) > file.Line(arrow.GetPreviousToken()))
            .Select(arrow => $"{file}:{file.Line(arrow)}");
}
