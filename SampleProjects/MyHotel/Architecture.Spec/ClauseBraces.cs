using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD.md 4g as an invariant over the syntax trees: a clause - the statement an `if`, an
/// `else`, a loop, a `using` or a `lock` holds - takes no braces when it is a simple statement, and takes
/// them when it is a structured statement or a sequence of statements, so an `else` never moves to an `if`
/// inside it. The `if` of an `else if` takes none; a declaration is neither, and the compiler keeps its
/// braces. That a `using` running to the end of its block is a declaration is IDE0063's. One fact per
/// project (Sources), and the shapes it tells apart.</summary>
public class ClauseBraces : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AClauseTakesBracesUnlessItIsASimpleStatement(string project) =>
        Breach(project).Is().Null();

    [Theory]
    [InlineData("if (a) F();")]
    [InlineData("if (a) F(); else if (b) G(); else H();")]
    [InlineData("if (a) { F(); G(); }")]
    [InlineData("if (a) { var x = F(); }")]
    [InlineData("foreach (var x in xs) { if (x) F(); }")]
    [InlineData("if (a) { while (b) { if (c) F(); } } else G();")]
    [InlineData("using (scope) return F();")]
    public void AStatementBracesItsClauses(string statement) => MisbracedIn(statement).Is().Empty();

    [Theory]
    [InlineData("if (a) { F(); }")]
    [InlineData("if (a) F(); else { G(); }")]
    [InlineData("while (a) if (b) F();")]
    [InlineData("if (a) { while (b) if (c) F(); } else G();")]
    [InlineData("foreach (var x in xs) foreach (var y in x) F();")]
    [InlineData("using (scope) { F(); }")]
    public void AStatementMisbracesOneClause(string statement) => MisbracedIn(statement).Has().OneItem();

    private static string? Breach(string project) =>
        Sites(project) is [var first, ..] and var sites
            ? $"{sites.Count} clauses brace a simple statement or leave any other bare, the first at {first}"
            : null;

    /// <summary>"file:line" for every misbraced clause in the project.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Source.Projects[project].Nodes<StatementSyntax>()
            .Where(clause => Misbraced(clause.Syntax))
            .Select(Reported)];

    private static IReadOnlyList<StatementSyntax> MisbracedIn(string statement) =>
        [.. SyntaxFactory.ParseStatement(statement).DescendantNodesAndSelf().OfType<StatementSyntax>()
            .Where(Misbraced)];

    /// <summary>A clause braced around one simple statement, or bare though it is none - but the `if` of an
    /// `else if`.</summary>
    private static bool Misbraced(StatementSyntax statement) =>
        IsAClause(statement) && statement switch
        {
            BlockSyntax { Statements: [var only] } => IsSimple(only),
            BlockSyntax => false,
            IfStatementSyntax { Parent: ElseClauseSyntax } => false,
            _ => !IsSimple(statement),
        };

    private static bool IsAClause(StatementSyntax statement) =>
        statement.Parent is IfStatementSyntax or ElseClauseSyntax or CommonForEachStatementSyntax
            or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax or UsingStatementSyntax
            or LockStatementSyntax;

    private static bool IsSimple(StatementSyntax statement) =>
        statement is ExpressionStatementSyntax or ReturnStatementSyntax or ThrowStatementSyntax
            or BreakStatementSyntax or ContinueStatementSyntax or YieldStatementSyntax;

    private static string Reported(SourceNode<StatementSyntax> clause) =>
        $"{clause.File}:{clause.File.Line(clause.Syntax)}";
}
