using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Primary expression: identifiers, literals, parenthesized expressions,
/// tuples, array literals, prefix unary fallbacks, and the <c>new</c>
/// expression (delegated to <see cref="NewExprRule"/>).
/// </summary>
internal static class PrimaryRule
{
    public static Expr Parse(TokenStream ts)
    {
        int save = ts.Pos;
        var t = ts.Peek();
        return (t.Kind, t.Text) switch
        {
            (TokenKind.Word, "new") => NewExprRule.Parse(ts),
            (TokenKind.Word, "true" or "false" or "null" or "default") => Consume(ts, new Literal(t.Text)),
            (TokenKind.Word, _) => Consume(ts, new Identifier(t.Text)),
            (TokenKind.Number or TokenKind.Char, _) => Consume(ts, new Literal(t.Text)),
            (TokenKind.String, _) => Consume(ts, StringLiteral(t.Text)),
            (TokenKind.Symbol, "(") => ParseParenOrTuple(ts, save),
            (TokenKind.Symbol, "[") => ParseArrayLit(ts, save),
            (TokenKind.Symbol, "-" or "+" or "!" or "~") => UnaryRule.Parse(ts),
            _ => Consume(ts, new Unknown(t.Text)),
        };
    }

    private static Expr Consume(TokenStream ts, Expr expr)
    {
        ts.Advance();
        return expr;
    }

    private static Expr StringLiteral(string text)
    {
        bool interpolated = text.StartsWith('$')
            || (text.Length > 1 && text[0] is '@' or '$' && text[1] == '$');
        return interpolated ? new InterpolatedString(text) : new Literal(text);
    }

    private static Expr ParseArrayLit(TokenStream ts, int save)
    {
        ts.Advance();                                       // consume '['
        if (!ts.TryParse("]", out var items))
            return new Unknown(ts.RawFrom(save));

        return new ArrayLit(ts.RawFrom(save), items);
    }

    /// Empty <c>()</c> is the unit tuple; a single item in parens unwraps to
    /// the inner expression; two or more becomes a <see cref="TupleExpr"/>.
    private static Expr ParseParenOrTuple(TokenStream ts, int save)
    {
        ts.Advance();                                       // consume '('
        if (!ts.TryParse(")", out var items))
            return new Unknown(ts.RawFrom(save));

        return items.Count == 1 ? items[0] : new TupleExpr(ts.RawFrom(save), items);
    }
}
