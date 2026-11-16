using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 6h as an invariant over the syntax trees: in a conditional the
/// condition and each branch take one line each, and so do a switch arm's pattern and expression; what needs
/// more is a method of its own. One fact per project (Sources).</summary>
public class ConditionalBranches : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void InAConditionalEachPartTakesOneLine(string project) =>
        Source.Projects[project].Files.SelectMany(PartsOverOneLine).Is().Empty();

    /// <summary>"file:line part lines" for every part of a conditional or switch arm that spans
    /// lines.</summary>
    private static IEnumerable<string> PartsOverOneLine(SourceFile file) =>
        file.Root.DescendantNodes().SelectMany(Parts)
            .Where(part => file.SpanOf(part.Node).Lines > 1)
            .Select(part => Reported(part, file));

    private static string Reported((string Name, SyntaxNode Node) part, SourceFile file) =>
        $"{file}:{file.Line(part.Node)} {part.Name} {file.SpanOf(part.Node).Lines} lines";

    private static IEnumerable<(string Name, SyntaxNode Node)> Parts(SyntaxNode node) =>
        node switch
        {
            ConditionalExpressionSyntax conditional => Parts(conditional),
            SwitchExpressionArmSyntax arm => [("pattern", arm.Pattern), ("arm", arm.Expression)],
            _ => [],
        };

    private static IReadOnlyList<(string Name, SyntaxNode Node)> Parts(ConditionalExpressionSyntax c) =>
        [("condition", c.Condition), ("then", c.WhenTrue), ("else", c.WhenFalse)];
}
