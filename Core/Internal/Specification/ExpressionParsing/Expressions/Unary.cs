namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Unary(string Raw, string Op, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];
}