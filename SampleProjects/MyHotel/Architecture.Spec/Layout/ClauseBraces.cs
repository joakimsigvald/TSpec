using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 4g as an invariant over the syntax trees: a clause - the statement an
/// `if`, an `else`, a loop, a `using` or a `lock` holds - takes no braces when it is a simple statement, and
/// takes them when it is a structured statement or a sequence of statements, so an `else` never moves to an
/// `if` inside it. The `if` of an `else if` takes none; a declaration is neither, and the compiler keeps its
/// braces. That a `using` running to the end of its block is a declaration is IDE0063's.</summary>
public class ClauseBraces : TSpec.Spec
{
    [Fact]
    public void AClauseTakesBracesUnlessItIsASimpleStatement() =>
        Source.Projects.Has().All(project => project.Nodes<StatementSyntax>().Has().None(Misbraced));

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

    private static IReadOnlyList<StatementSyntax> MisbracedIn(string statement) =>
        [.. SyntaxFactory.ParseStatement(statement).DescendantNodesAndSelf().OfType<StatementSyntax>()
            .Where(Misbraced)];

    private static bool Misbraced(SourceNode<StatementSyntax> clause) => Misbraced(clause.Syntax);

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
}
