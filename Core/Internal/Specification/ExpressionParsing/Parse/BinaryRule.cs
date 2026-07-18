using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Pratt-style binary precedence climb. <c>is</c> / <c>as</c> sit at the
/// relational level and take a type ref on the right.
/// </summary>
internal static class BinaryRule
{
    public const int MinPrecedence = 1;
    private const int _relationalPrecedence = 5;

    // (operator, precedence, right-associative)
    private static readonly (string Op, int Prec, bool RightAssoc)[] _ops =
    [
        ("??", 1, true),
        ("||", 2, false), ("|", 2, false),
        ("&&", 3, false), ("&", 3, false),
        ("==", 4, false), ("!=", 4, false),
        ("<", 5, false), (">", 5, false), ("<=", 5, false), (">=", 5, false),
        ("+", 6, false), ("-", 6, false),
        ("*", 7, false), ("/", 7, false), ("%", 7, false),
    ];

    public static Expr Parse(TokenStream ts, int minPrec)
    {
        int save = ts.Pos;
        var left = UnaryRule.Parse(ts);
        while (true)
        {
            if (IsTypeOp(ts) && _relationalPrecedence >= minPrec)
            {
                left = ParseIsAs(ts, save, left);
                continue;
            }
            if (Match(ts.Peek(), minPrec) is not (var op, var prec, var rightAssoc))
                return left;

            ts.Advance();
            left = new Binary(ts.RawFrom(save), op, left, Parse(ts, rightAssoc ? prec : prec + 1));
        }
    }

    private static bool IsTypeOp(TokenStream ts) => ts.IsWord("is") || ts.IsWord("as");

    private static IsAs ParseIsAs(TokenStream ts, int save, Expr left)
    {
        string op = ts.Peek().Text;
        ts.Advance();
        return new(ts.RawFrom(save), op, left, TypeRefRule.ConsumeTypeRef(ts));
    }

    private static (string Op, int Prec, bool RightAssoc)? Match(Token t, int minPrec)
    {
        if (t.Kind != TokenKind.Symbol)
            return null;

        foreach (var op in _ops)
            if (op.Op == t.Text && op.Prec >= minPrec)
                return op;

        return null;
    }
}
