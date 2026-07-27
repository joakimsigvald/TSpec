namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Unary(string Raw, string Op, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];

    /// <c>await</c> describes how the operand is obtained, not what it is, so it
    /// peels away here rather than reaching any description.
    public override Expr WithoutNoise() => IsNoiseOperator ? Operand.WithoutNoise() : this;
    public override string AsPath() => IsNoiseOperator ? Operand.AsPath() : Raw;
    public override string ToSource()
        => IsNoiseOperator ? Operand.ToSource() : $"{Op}{Operand.ToSource()}";

    private bool IsNoiseOperator => Op == "await";
}