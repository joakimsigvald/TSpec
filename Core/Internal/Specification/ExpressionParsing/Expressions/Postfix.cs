namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Postfix(string Raw, string Op, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];

    /// The null-forgiving operator is semantically transparent, so a mention
    /// wrapped in <c>!</c> is still a mention. <c>++</c>/<c>--</c> are not.
    public override Mention? AsMention() => Op == "!" ? Operand.AsMention() : null;
}