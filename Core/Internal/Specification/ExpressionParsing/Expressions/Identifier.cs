namespace TSpec.Internal.Specification.ExpressionParsing.Expressions;

internal sealed record Identifier(string Raw, string Name) : Expr(Raw)
{
    /// Convenience for the (common) case where the identifier's source text
    /// IS its name — no surrounding whitespace or trivia.
    public Identifier(string name) : this(name, name) { }

    public override string AsPath() => Name;
}