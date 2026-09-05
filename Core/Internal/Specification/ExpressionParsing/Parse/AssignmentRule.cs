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
        var left = ConditionalRule.Parse(ts);
        if (ts.Peek() is not { Kind: TokenKind.Symbol } op || !_ops.Contains(op.Text))
            return left;

        ts.Advance();
        var right = Parse(ts);
        return new Assign($"{left.Raw} {op.Text} {right.Raw}", op.Text, left, right);
    }
}