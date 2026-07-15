namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record NamedArg(string Raw, string Name, Expr Value) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Value];
}