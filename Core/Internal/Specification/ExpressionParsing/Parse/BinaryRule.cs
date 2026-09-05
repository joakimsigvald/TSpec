using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Pratt-style binary precedence climb. <c>is</c> / <c>as</c> sit at the
/// relational level and take a type ref on the right.
/// </summary>
internal static class BinaryRule
{
    public const int MinPrecedence = 0;
    private const int RelationalPrecedence = 4;

    /// <summary>
    /// The operators by precedence, loosest binding first
    /// </summary>
    private static readonly string[][] _ops =
    [
        ["??"],
        ["||", "|"],
        ["&&", "&"],
        ["==", "!="],
        ["<", ">", "<=", ">="],
        ["+", "-"],
        ["*", "/", "%"],
    ];

    /// The same table, indexed the way the parser asks it: by the operator it just read.
    private static readonly Dictionary<string, int> _precedenceByOp = _ops
        .SelectMany((ops, prec) => ops.Select(op => (Op: op, Prec: prec)))
        .ToDictionary(entry => entry.Op, entry => entry.Prec);

    public static Expr Parse(TokenStream ts, int minPrec)
    {
        int save = ts.Pos;
        var left = UnaryRule.Parse(ts);
        while (true)
        {
            if (IsTypeOp(ts) && RelationalPrecedence >= minPrec)
            {
                left = ParseIsAs(ts, save, left);
                continue;
            }
            var op = ts.Peek();
            if (Precedence(op, minPrec) is not { } prec)
                return left;

            ts.Advance();
            var right = Parse(ts, prec + 1);
            left = new Binary($"{left.Raw} {op.Text} {right.Raw}", op.Text, left, right);
        }
    }

    private static bool IsTypeOp(TokenStream ts) => ts.IsWord("is") || ts.IsWord("as");

    private static IsAs ParseIsAs(TokenStream ts, int save, Expr left)
    {
        string op = ts.Peek().Text;
        ts.Advance();
        return new(ts.RawFrom(save), op, left, TypeRefRule.ConsumeTypeRef(ts));
    }

    private static int? Precedence(Token t, int minPrec)
        => t.Kind == TokenKind.Symbol
            && _precedenceByOp.TryGetValue(t.Text, out var prec)
            && prec >= minPrec
        ? prec : null;
}
