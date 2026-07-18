using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Comma-separated expression list bounded by a terminator. Implemented as
/// an extension on <see cref="TokenStream"/> so call sites stay terse, while
/// the dependency on <see cref="LambdaRule"/> (and through it the full
/// expression grammar) lives in the Parse namespace, not in Tokenize.
/// </summary>
internal static class ParseList
{
    /// <summary>
    /// Parses comma-separated expressions until <paramref name="terminator"/>.
    /// Returns true if the terminator was consumed; false if missing.
    /// </summary>
    public static bool TryParse(this TokenStream ts, string terminator, out IReadOnlyList<Expr> items)
    {
        var list = new List<Expr>();
        if (!ts.IsSym(terminator))
        {
            while (true)
            {
                list.Add(ParseItem(ts));
                if (!ts.AcceptSym(",")) break;
            }
        }
        items = list;
        return ts.AcceptSym(terminator);
    }

    /// A list item is either a plain expression or a named argument
    /// (<c>name: value</c>). The colon can only mean a named argument at the
    /// start of an item — a ternary's colon is always preceded by <c>?</c>.
    private static Expr ParseItem(TokenStream ts)
    {
        if (ts.Peek() is not { Kind: TokenKind.Word } name
            || ts.Peek(1) is not { Kind: TokenKind.Symbol, Text: ":" })
            return LambdaRule.Parse(ts);

        int save = ts.Pos;
        ts.Advance();
        ts.Advance();
        var value = LambdaRule.Parse(ts);
        return new NamedArg(ts.RawFrom(save), name.Text, value);
    }
}