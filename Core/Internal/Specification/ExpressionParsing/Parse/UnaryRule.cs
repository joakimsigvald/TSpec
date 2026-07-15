using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Prefix-unary operators and parenthesized cast expressions.
/// </summary>
internal static class UnaryRule
{
    public static Expr Parse(TokenStream ts)
    {
        int save = ts.Pos;
        if (ts.Peek().Kind == TokenKind.Symbol &&
            ts.Peek().Text is "!" or "-" or "+" or "~" or "++" or "--")
        {
            string op = ts.Peek().Text;
            ts.Advance();
            return new Unary(ts.RawFrom(save), op, Parse(ts));
        }
        if (ts.IsSym("(") && LooksLikeCast(ts))
        {
            int castSave = ts.Pos;
            ts.Advance();                                   // consume '('
            string typeName = TypeRefRule.ConsumeTypeRef(ts);
            if (ts.AcceptSym(")")) 
                return new Cast(ts.RawFrom(castSave), typeName, Parse(ts));

            ts.Pos = castSave;
        }
        return PostfixRule.Parse(ts);
    }

    private static bool LooksLikeCast(TokenStream ts) => ts.PeekAhead(stream =>
    {
        stream.Advance();                                   // consume '('
        if (stream.Peek().Kind != TokenKind.Word) return false;
        stream.ScanBalanced(t => t.Kind == TokenKind.Symbol && t.Text is ")" or ",");
        if (!stream.IsSym(")")) 
            return false;

        stream.Advance();                                   // consume ')'
        var nxt = stream.Peek();
        return nxt.Kind is TokenKind.Word or TokenKind.Number
            || (nxt.Kind == TokenKind.Symbol && nxt.Text is "(" or "-" or "!" or "~");
    });
}