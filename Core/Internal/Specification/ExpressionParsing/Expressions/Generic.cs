namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Generic(string Raw, Expr Target, IReadOnlyList<Expr> TypeArgs) : Expr(Raw)
{
    public override IEnumerable<Expr> Children => TypeArgs.Prepend(Target);
    public override string AsPath() =>
        $"{Target.AsPath()}<{string.Join(", ", TypeArgs.Select(t => t.Raw))}>";

    public override Mention? AsMention() => Target is Identifier id && TypeArgs.Count > 0
        ? new Mention(Raw, id.Name, string.Join(", ", TypeArgs.Select(t => t.Raw)), null)
        : null;
}