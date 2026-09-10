namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Cast(string Raw, string TypeName, Expr Operand) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Operand];
    public override string ToSource() => $"({TypeName}){Operand.ToSource()}";
    public bool IsNullOfType() => Operand.WithoutNoise() is Literal { Raw: "null" };
}