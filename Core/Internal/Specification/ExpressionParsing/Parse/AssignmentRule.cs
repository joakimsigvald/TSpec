using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Right-associative assignment level: <c>=</c>, <c>+=</c>, <c>-=</c>,
/// <c>*=</c>, <c>/=</c>, <c>%=</c>, <c>&amp;=</c>, <c>|=</c>, <c>^=</c>.
/// </summary>
internal static class AssignmentRule
{
    private static readonly string[] _ops =
        ["=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^="];

    public static Expr Parse(TokenStream ts)
    {
        int save = ts.Pos;
        var left = ConditionalRule.Parse(ts);
        if (ts.Peek().Kind != TokenKind.Symbol || !_ops.Contains(ts.Peek().Text))
            return left;

        string op = ts.Peek().Text;
        ts.Advance();
        return new Assign(ts.RawFrom(save), op, left, Parse(ts));
    }
}