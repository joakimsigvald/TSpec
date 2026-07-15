namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record IsAs(string Raw, string Op, Expr Operand, string TypeName) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];
}