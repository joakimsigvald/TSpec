using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Entry-point grammar rule: parses a full expression. When the input begins
/// with a lambda head ("p =&gt; ..." or "(p, q) =&gt; ..."), wraps the body in a
/// <see cref="Lambda"/>; otherwise delegates to <see cref="AssignmentRule"/>.
/// If the body fails to parse cleanly, the body's raw source is preserved as
/// an <see cref="Unknown"/> node.
/// </summary>
internal static class LambdaRule
{
    public static Expr Parse(TokenStream ts)
    {
        int save = ts.Pos;
        if (!TryParams(ts, out var ps) || !ts.AcceptSym("=>"))
        {
            ts.Pos = save;
            return AssignmentRule.Parse(ts);
        }

        int bodyStart = ts.Pos < ts.Count ? ts.TokenStart(ts.Pos) : ts.Source.Length;
        int bodySave = ts.Pos;
        var body = Parse(ts);

        // If parsing left tokens unconsumed that aren't an outer-context boundary,
        // the body wasn't fully recognized — capture its raw source instead.
        if (ts.Peek().Kind != TokenKind.EndOfInput && !IsBoundary(ts.Peek()))
        {
            ts.Pos = bodySave;
            ts.ScanBalanced(IsBoundary);
            int bodyEnd = ts.Pos < ts.Count ? ts.TokenStart(ts.Pos) : ts.Source.Length;
            body = new Unknown(ts.Source[bodyStart..bodyEnd].Trim());
        }
        return new Lambda(ts.RawFrom(save), ps, body);
    }

    private static bool IsBoundary(Token t)
        => t.Kind == TokenKind.Symbol && t.Text is "," or ")" or "]" or "}";

    /// The head may carry an <c>async</c> modifier and an explicit return type
    /// (<c>async Task&lt;int&gt; (_) =&gt; ...</c>). Both are consumed and dropped —
    /// they describe how the lambda runs, not what the subject does. Every
    /// branch below backtracks, and <see cref="Parse"/> resets on failure, so a
    /// wrong guess costs nothing.
    private static bool TryParams(TokenStream ts, out IReadOnlyList<string> ps)
    {
        ps = [];
        if (ts.IsWord("async"))
        {
            ts.Advance();
            TrySkipReturnType(ts);
        }
        return TrySingleParam(ts, out ps) || TryParenParams(ts, out ps);
    }

    /// A return type is a type reference immediately followed by a parenthesised
    /// parameter list. Only recognised after <c>async</c>: assertion phrases are
    /// parsed with a word prefixed to the expression (<c>"by (it, i) => …"</c>),
    /// and that shape is indistinguishable from a bare return type.
    private static void TrySkipReturnType(TokenStream ts)
    {
        int save = ts.Pos;
        if (ts.Peek().Kind != TokenKind.Word)
            return;

        while (ts.Peek() is { Kind: TokenKind.Word }
            or { Kind: TokenKind.Symbol, Text: "." or "<" or ">" or "," or "?" or "[" or "]" })
            ts.Advance();

        if (!ts.IsSym("("))
            ts.Pos = save;
    }

    /// Matches <c>x =&gt;</c> — a single bare identifier followed by <c>=&gt;</c>.
    private static bool TrySingleParam(TokenStream ts, out IReadOnlyList<string> ps)
    {
        ps = [];
        if (ts.Peek() is not { Kind: TokenKind.Word } param || ts.Peek(1).Text != "=>")
            return false;

        ps = [param.Text];
        ts.Advance();
        return true;
    }

    /// Matches <c>(x, y, ...) =&gt;</c> — a parenthesised parameter list followed
    /// by <c>=&gt;</c>. Backtracks fully on any mismatch.
    private static bool TryParenParams(TokenStream ts, out IReadOnlyList<string> ps)
    {
        ps = [];
        if (!ts.IsSym("(")) 
            return false;

        int save = ts.Pos;
        ts.Advance();
        var list = new List<string>();
        while (!ts.IsSym(")"))
        {
            if (ts.Peek() is not { Kind: TokenKind.Word } param)
            {
                ts.Pos = save;
                return false;
            }

            list.Add(param.Text);
            ts.Advance();
            if (!ts.AcceptSym(","))
                break;
        }
        if (!ts.AcceptSym(")") || !ts.IsSym("=>"))
        {
            ts.Pos = save;
            return false;
        }

        ps = list;
        return true;
    }
}