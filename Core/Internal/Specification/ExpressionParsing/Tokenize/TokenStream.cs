namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

/// <summary>
/// Token cursor over a source string. Holds the parse position and exposes
/// low-level primitives used by every grammar rule.
/// </summary>
internal sealed class TokenStream(string source)
{
    public string Source { get; } = source;
    private readonly List<Token> _tokens = Tokenizer.Tokenize(source);
    public int Pos { get; set; }
    public int Count => _tokens.Count;

    public Token Peek(int offset = 0) => _tokens[Math.Min(Pos + offset, _tokens.Count - 1)];
    public void Advance() => Pos++;

    /// Runs <paramref name="scan"/> as a speculative read: any position
    /// changes are rolled back before returning, so the caller sees the same
    /// state as before the call.
    public T PeekAhead<T>(Func<TokenStream, T> scan)
    {
        int save = Pos;
        try { return scan(this); }
        finally { Pos = save; }
    }
    public bool IsSym(string s) => Peek().Kind == TokenKind.Symbol && Peek().Text == s;
    public bool IsWord(string s) => Peek().Kind == TokenKind.Word && Peek().Text == s;
    public bool AcceptSym(string s) { if (!IsSym(s)) return false; Advance(); return true; }

    public int TokenStart(int i) => _tokens[Math.Clamp(i, 0, _tokens.Count - 1)].Start;
    public int TokenEnd(int i) => _tokens[Math.Clamp(i, 0, _tokens.Count - 1)].End;

    public string RawFrom(int startTok)
    {
        if (startTok >= _tokens.Count) return string.Empty;
        int start = _tokens[startTok].Start;
        int end = Pos > 0 ? _tokens[Pos - 1].End : start;
        return end > start ? Source[start..end] : string.Empty;
    }

    /// <summary>
    /// Walks tokens with bracket-depth tracking across (), [], &lt;&gt;, {}.
    /// Stops when <paramref name="stopAtZero"/> returns true at depth 0, or at EOF.
    /// Returns the source text covered. Advances <see cref="Pos"/> to the stop token.
    /// </summary>
    public string ScanBalanced(Func<Token, bool> stopAtZero)
    {
        int startTok = Pos;
        int depth = 0;
        while (Peek().Kind != TokenKind.EndOfInput)
        {
            var t = Peek();
            if (depth == 0 && stopAtZero(t)) break;
            if (t.Kind == TokenKind.Symbol)
            {
                if (t.Text is "(" or "[" or "<" or "{") depth++;
                else if (t.Text is ")" or "]" or ">" or "}") depth--;
            }
            Advance();
        }
        if (startTok >= _tokens.Count) return string.Empty;
        int s = _tokens[startTok].Start;
        int e = Pos > startTok ? _tokens[Pos - 1].End : s;
        return e > s ? Source[s..e] : string.Empty;
    }
}