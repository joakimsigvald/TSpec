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

    /// How tightly an operator binds, for a printer rebuilding an expression from its operands.
    /// <c>is</c> and <c>as</c> are not in the table and sit at the relational level.
    public static int PrecedenceOf(string op)
        => _precedenceByOp.TryGetValue(op, out var prec) ? prec : RelationalPrecedence;

    public static Expr Parse(TokenStream ts, int minPrec)
    {
        var left = UnaryRule.Parse(ts);
        while (true)
        {
            if (IsTypeOp(ts) && RelationalPrecedence >= minPrec)
            {
                left = ParseIsAs(ts, left);
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

    private static IsAs ParseIsAs(TokenStream ts, Expr left)
    {
        string op = ts.Peek().Text;
        ts.Advance();
        var typeName = TypeRefRule.ConsumeTypeRef(ts);
        return new($"{left.Raw} {op} {typeName}", op, left, typeName);
    }

    private static int? Precedence(Token t, int minPrec)
        => t.Kind == TokenKind.Symbol
            && _precedenceByOp.TryGetValue(t.Text, out var prec)
            && prec >= minPrec
        ? prec : null;
}
