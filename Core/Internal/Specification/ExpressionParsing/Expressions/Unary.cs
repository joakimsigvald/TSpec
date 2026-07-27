namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Unary(string Raw, string Op, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];

    /// <c>await</c> describes how the operand is obtained, not what it is, so it
    /// peels away here rather than reaching any description.
    public override Expr WithoutNoise() => IsNoiceOperator ? Operand.WithoutNoise() : this;
    public override string AsPath() => IsNoiceOperator ? Operand.AsPath() : Raw;

    private bool IsNoiceOperator => Op == "await";
}