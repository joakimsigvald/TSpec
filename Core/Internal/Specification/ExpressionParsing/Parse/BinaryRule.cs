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
            if ((ts.IsWord("is") || ts.IsWord("as")) && _relationalPrecedence >= minPrec)
            {
                string op = ts.Peek().Text;
                ts.Advance();
                left = new IsAs(ts.RawFrom(save), op, left, TypeRefRule.ConsumeTypeRef(ts));
                continue;
            }
            var matched = Match(ts.Peek(), minPrec);
            if (matched is null) 
                break;

            ts.Advance();
            int nextMin = matched.Value.RightAssoc ? matched.Value.Prec : matched.Value.Prec + 1;
            left = new Binary(ts.RawFrom(save), matched.Value.Op, left, Parse(ts, nextMin));
        }
        return left;
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