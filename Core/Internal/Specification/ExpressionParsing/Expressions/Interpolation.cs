namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

/// <summary>
/// One <c>{…}</c> hole of an interpolated string. <paramref name="Suffix"/> keeps any alignment or
/// format specifier (<c>{value,10:N2}</c>) exactly as written — it is formatting, not an expression.
/// </summary>
internal sealed record Interpolation(string Raw, Expr Value, string Suffix) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => [Value];
    public override string ToSource() => $"{{{Value.ToSource()}{Suffix}}}";
}
