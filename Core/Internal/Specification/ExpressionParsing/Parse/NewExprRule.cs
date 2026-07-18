using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// <c>new TypeName(...args)</c>, <c>new TypeName { ...init }</c>,
/// <c>new TypeName(...args) { ...init }</c>, <c>new int[] { ... }</c>,
/// and target-typed <c>new(...args)</c>.
/// </summary>
internal static class NewExprRule
{
    public static Expr Parse(TokenStream ts)
    {
        int start = ts.Pos;
        ts.Advance();                                       // consume 'new'
        return TryParseTypeName(ts, out var typeName)
            && TryParseArguments(ts, out var args)
            && TryParseInitializer(ts, out var initializer)
            ? new New(ts.RawFrom(start), typeName, args, initializer)
            : new Unknown(ts.RawFrom(start));
    }

    private static bool TryParseTypeName(TokenStream ts, out string? typeName)
    {
        typeName = null;
        if (ts.Peek().Kind != TokenKind.Word || ts.Peek().Text == "with")
            return true;

        int start = ts.Pos;
        ts.Advance();
        while (ts.AcceptSym("."))
        {
            if (ts.Peek().Kind != TokenKind.Word)
                return false;

            ts.Advance();
        }
        if (ts.IsSym("<") && !TypeRefRule.TryParseGenericArgs(ts, out _))
            return false;

        typeName = ts.Source[ts.TokenStart(start)..ts.TokenEnd(ts.Pos - 1)];
        return true;
    }

    private static bool TryParseArguments(TokenStream ts, out IReadOnlyList<Expr> arguments)
    {
        arguments = [];
        if (ts.AcceptSym("("))
            return ts.TryParse(")", out arguments);

        if (ts.AcceptSym("["))
            return ts.TryParse("]", out arguments);

        return true;
    }

    private static bool TryParseInitializer(TokenStream ts, out IReadOnlyList<Expr>? initializer)
    {
        initializer = null;
        if (!ts.AcceptSym("{"))
            return true;

        if (!ts.TryParse("}", out var expressions))
            return false;

        initializer = expressions;
        return true;
    }
}
