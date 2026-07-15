using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Postfix loop: member access (<c>.</c> / <c>?.</c>), generic application,
/// invocation, indexing, <c>with { }</c> blocks, and the postfix operators
/// <c>++</c> / <c>--</c> / <c>!</c> (null-forgiving). Each handler returns
/// one of three outcomes — <see cref="Step"/> tracks them:
/// <c>null</c> = not my shape, try the next handler;
/// <c>Step.More(e)</c> = consumed input, keep looping with <c>e</c>;
/// <c>Step.Stop(e)</c> = consumed input but the bracket never closed,
/// so wrap up with <c>e</c> and exit the loop.
/// </summary>
internal static class PostfixRule
{
    private sealed record Step(Expr Result, bool Final)
    {
        /// Consumed input and produced <paramref name="e"/> — keep looping.
        public static Step More(Expr e) => new(e, Final: false);

        /// Consumed input and produced <paramref name="e"/>, but the bracket
        /// never closed — wrap it up and stop the loop.
        public static Step Stop(Expr e) => new(e, Final: true);
    }

    public static Expr Parse(TokenStream ts)
    {
        int save = ts.Pos;
        var expr = PrimaryRule.Parse(ts);
        while (true)
        {
            var step = TryMember(ts, save, expr)
                ?? TryGeneric(ts, save, expr)
                ?? TryInvocation(ts, save, expr)
                ?? TryIndexing(ts, save, expr)
                ?? TryWith(ts, save, expr)
                ?? TryPostfixOp(ts, save, expr);
            if (step is null) return expr;
            if (step.Final) return step.Result;
            expr = step.Result;
        }
    }

    private static Step? TryMember(TokenStream ts, int save, Expr expr)
    {
        if (ts.Peek().Kind != TokenKind.Symbol || ts.Peek().Text is not ("." or "?."))
            return null;

        bool nullConditional = ts.Peek().Text == "?.";
        ts.Advance();
        if (ts.Peek().Kind != TokenKind.Word)
            return null;

        string name = ts.Peek().Text;
        ts.Advance();
        return Step.More(new Member(ts.RawFrom(save), expr, name, nullConditional));
    }

    private static Step? TryGeneric(TokenStream ts, int save, Expr expr)
    {
        if (!ts.IsSym("<") || !TypeRefRule.CanBeGenericApplication(ts, expr))
            return null;
        if (!TypeRefRule.TryParseGenericArgs(ts, out var typeArgs))
            return null;
        return Step.More(new Generic(ts.RawFrom(save), expr, typeArgs));
    }

    private static Step? TryInvocation(TokenStream ts, int save, Expr expr)
        => TryBracketed(ts, save, "(", ")", (raw, target, args) => new Call(raw, target, args), expr);

    private static Step? TryIndexing(TokenStream ts, int save, Expr expr)
        => TryBracketed(ts, save, "[", "]", (raw, target, args) => new IndexExpr(raw, target, args), expr);

    private static Step? TryWith(TokenStream ts, int save, Expr expr)
    {
        if (!ts.IsWord("with")) return null;

        int withSave = ts.Pos;
        ts.Advance();
        if (!ts.AcceptSym("{"))
        {
            ts.Pos = withSave;
            return null;
        }
        bool closed = ts.TryParse("}", out var inits);
        var next = new With(ts.RawFrom(save), expr, inits);
        return closed ? Step.More(next) : Step.Stop(next);
    }

    private static Step? TryPostfixOp(TokenStream ts, int save, Expr expr)
    {
        if (ts.Peek().Kind != TokenKind.Symbol || ts.Peek().Text is not ("++" or "--" or "!"))
            return null;

        string op = ts.Peek().Text;
        ts.Advance();
        return Step.More(new Postfix(ts.RawFrom(save), op, expr));
    }

    private static Step? TryBracketed(TokenStream ts, int save, string open, string close,
        Func<string, Expr, IReadOnlyList<Expr>, Expr> build, Expr expr)
    {
        if (!ts.AcceptSym(open)) return null;
        bool closed = ts.TryParse(close, out var args);
        var next = build(ts.RawFrom(save), expr, args);
        return closed ? Step.More(next) : Step.Stop(next);
    }
}
