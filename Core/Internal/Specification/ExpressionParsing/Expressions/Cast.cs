namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Cast(string Raw, string TypeName, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];
}