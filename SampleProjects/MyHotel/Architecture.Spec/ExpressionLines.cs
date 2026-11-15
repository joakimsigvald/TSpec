using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD.md 6g as an invariant over the syntax trees: a statement's body below its head, a
/// switch expression one arm per line, a conditional's parts on lines of their own unless the whole fits on
/// one. One fact per project (Sources).</summary>
public class ExpressionLines : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void EachExpressionHasALineOfItsOwn(string project) =>
        Source.Projects[project].Files.SelectMany(SharedLines).Is().Empty();

    /// <summary>"file:line what" for every expression written on another's line.</summary>
    private static IEnumerable<string> SharedLines(SourceFile file) =>
        file.Root.DescendantNodes()
            .Select(node => (Node: node, What: Shared(node, file)))
            .Where(found => found.What is not null)
            .Select(found => $"{file}:{file.Line(found.Node)} {found.What}");

    private static string? Shared(SyntaxNode node, SourceFile file) =>
        node switch
        {
            IfStatementSyntax i when file.Beside(i.IfKeyword, i.Statement) => "the if's body",
            ElseClauseSyntax e when ElseBodyBeside(e, file) => "the else's body",
            ForEachStatementSyntax f when file.Beside(f.ForEachKeyword, f.Statement) => "the foreach's body",
            ForStatementSyntax f when file.Beside(f.ForKeyword, f.Statement) => "the for's body",
            WhileStatementSyntax w when file.Beside(w.WhileKeyword, w.Statement) => "the while's body",
            SwitchExpressionSyntax s when ArmsShareALine(s, file) => "two switch arms",
            ConditionalExpressionSyntax c when PartsShareALine(c, file) => "the conditional's parts",
            _ => null,
        };

    /// <summary>An `else if` keeps the if on the else's line by convention.</summary>
    private static bool ElseBodyBeside(ElseClauseSyntax e, SourceFile file) =>
        e.Statement is not IfStatementSyntax && file.Beside(e.ElseKeyword, e.Statement);

    private static bool ArmsShareALine(SwitchExpressionSyntax s, SourceFile file)
    {
        List<int> starts = [.. s.Arms.Select(arm => file.Line(arm))];
        return starts.Distinct().Count() < starts.Count || starts.Contains(file.Line(s.OpenBraceToken));
    }

    /// <summary>The whole on one line is fine; spanning lines, the condition and each branch start on their
    /// own.</summary>
    private static bool PartsShareALine(ConditionalExpressionSyntax c, SourceFile file)
    {
        int[] starts = [file.Line(c.Condition), file.Line(c.QuestionToken), file.Line(c.ColonToken)];
        return file.SpanOf(c).Lines > 1 && starts.Distinct().Count() < 3;
    }
}
