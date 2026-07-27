namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Postfix(string Raw, string Op, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];

    /// The null-forgiving operator tells the compiler something; it does nothing
    /// at run time and states nothing about the subject, so it peels away.
    /// <c>++</c>/<c>--</c> are real operations and stay.
    public override Expr WithoutNoise() => IsNoiseOperator ? Operand.WithoutNoise() : this;
    public override string AsPath() => IsNoiseOperator ? Operand.AsPath() : Raw;
    public override string ToSource()
        => IsNoiseOperator ? Operand.ToSource() : $"{Operand.ToSource()}{Op}";

    /// Being transparent, a mention wrapped in <c>!</c> is still a mention.
    public override Mention? AsMention() => IsNoiseOperator ? Operand.AsMention() : null;

    private bool IsNoiseOperator => Op == "!";
}