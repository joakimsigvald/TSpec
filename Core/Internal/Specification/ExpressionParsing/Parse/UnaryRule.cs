using System.Diagnostics.CodeAnalysis;
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
        if (ts.Peek() is { Kind: TokenKind.Symbol, Text: "!" or "-" or "+" or "~" or "++" or "--" } op)
        {
            ts.Advance();
            return new Unary(ts.RawFrom(save), op.Text, Parse(ts));
        }
        if (ts.IsSym("(") && LooksLikeCast(ts) && TryParseCast(ts, out var cast))
            return cast;

        return PostfixRule.Parse(ts);
    }

    private static bool TryParseCast(TokenStream ts, [NotNullWhen(true)] out Expr? cast)
    {
        cast = null;
        int save = ts.Pos;
        ts.Advance();                                       // consume '('
        string typeName = TypeRefRule.ConsumeTypeRef(ts);
        if (!ts.AcceptSym(")"))
        {
            ts.Pos = save;
            return false;
        }
        cast = new Cast(ts.RawFrom(save), typeName, Parse(ts));
        return true;
    }

    private static bool LooksLikeCast(TokenStream ts) => ts.PeekAhead(stream =>
    {
        stream.Advance();                                   // consume '('
        if (stream.Peek().Kind != TokenKind.Word)
            return false;

        stream.ScanBalanced(t => t.Kind == TokenKind.Symbol && t.Text is ")" or ",");
        if (!stream.IsSym(")"))
            return false;

        stream.Advance();                                   // consume ')'
        var nxt = stream.Peek();
        return nxt.Kind is TokenKind.Word or TokenKind.Number
            || (nxt.Kind == TokenKind.Symbol && nxt.Text is "(" or "-" or "!" or "~");
    });
}
