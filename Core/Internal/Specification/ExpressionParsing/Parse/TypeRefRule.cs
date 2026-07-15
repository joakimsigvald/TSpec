using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Type-reference scanning: deciding whether a <c>&lt;</c> opens generic
/// arguments, consuming the arg list, and capturing a single type ref as
/// raw text (used by generics, casts, and <c>is</c>/<c>as</c>).
/// </summary>
internal static class TypeRefRule
{
    public static bool CanBeGenericApplication(TokenStream ts, Expr target)
    {
        if (target is not Identifier and not Member) return false;
        return ts.PeekAhead(stream =>
        {
            if (!stream.AcceptSym("<")) return false;
            stream.ScanBalanced(t => t.Kind == TokenKind.Symbol &&
                t.Text is ">" or ";" or "{" or "}");
            if (!stream.IsSym(">")) return false;
            stream.Advance();
            var follow = stream.Peek();
            if (follow.Kind == TokenKind.EndOfInput) return true;
            return follow.Kind == TokenKind.Symbol
                && follow.Text is "(" or "." or "," or ")" or "]" or ";" or "?" or ":" or "==" or "!=";
        });
    }

    public static bool TryParseGenericArgs(TokenStream ts, out IReadOnlyList<Expr> typeArgs)
    {
        int save = ts.Pos;
        if (!ts.AcceptSym("<")) { typeArgs = []; return false; }
        var list = new List<Expr>();
        if (!ts.IsSym(">"))
        {
            while (true)
            {
                var typeText = ConsumeTypeRef(ts);
                list.Add(new Identifier(typeText));
                if (!ts.AcceptSym(",")) break;
            }
        }
        if (!ts.AcceptSym(">")) { ts.Pos = save; typeArgs = []; return false; }
        typeArgs = list;
        return true;
    }

    public static string ConsumeTypeRef(TokenStream ts)
        => ts.ScanBalanced(t => t.Kind == TokenKind.Symbol &&
            (t.Text is ")" or "]" or ">" || t.Text == ","));
}