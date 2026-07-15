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

        if (t.Kind == TokenKind.Word)
        {
            if (t.Text == "new") return NewExprRule.Parse(ts);
            ts.Advance();
            if (t.Text is "true" or "false" or "null" or "default") return new Literal(t.Text);
            return new Identifier(t.Text);
        }
        if (t.Kind is TokenKind.Number or TokenKind.Char)
        {
            ts.Advance();
            return new Literal(t.Text);
        }
        if (t.Kind == TokenKind.String)
        {
            ts.Advance();
            bool interpolated = t.Text.StartsWith('$')
                || (t.Text.Length > 1 && t.Text[0] is '@' or '$' && t.Text[1] == '$');
            return interpolated ? new InterpolatedString(t.Text) : new Literal(t.Text);
        }
        if (t.Kind == TokenKind.Symbol)
        {
            if (t.Text == "(") return ParseParenOrTuple(ts, save);
            if (t.Text == "[")
            {
                ts.Advance();
                if (!ts.TryParse("]", out var items)) return new Unknown(ts.RawFrom(save));
                return new ArrayLit(ts.RawFrom(save), items);
            }
            if (t.Text is "-" or "+" or "!" or "~") return UnaryRule.Parse(ts);
        }
        ts.Advance();
        return new Unknown(t.Text);
    }

    /// Empty <c>()</c> is the unit tuple; a single item in parens unwraps to
    /// the inner expression; two or more becomes a <see cref="TupleExpr"/>.
    private static Expr ParseParenOrTuple(TokenStream ts, int save)
    {
        ts.Advance();                                       // consume '('
        if (!ts.TryParse(")", out var items)) return new Unknown(ts.RawFrom(save));
        return items.Count == 1 ? items[0] : new TupleExpr(ts.RawFrom(save), items);
    }
}