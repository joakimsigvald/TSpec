namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

internal static class Tokenizer
{
    private static readonly string[] _multiCharOps =
    [
        "...",
        "=>", "==", "!=", "<=", ">=", "&&", "||", "??", "?.", "..",
        "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
        "++", "--",
    ];

    public static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < input.Length)
        {
            if (char.IsWhiteSpace(input[i])) { i++; continue; }
            var token = ReadNext(input, i);
            tokens.Add(token);
            i = token.End;
        }
        tokens.Add(new Token(TokenKind.EndOfInput, "", input.Length, input.Length));
        return tokens;
    }

    private static Token ReadNext(string input, int start)
    {
        char c = input[start];
        return ReadWord(c, input, start)
            ?? ReadNumber(c, input, start)
            ?? ReadString(input, start)
            ?? ReadChar(input, start)
            ?? ReadSymbolToken(input, start);
    }

    private static Token? ReadWord(char c, string input, int start)
    {
        if (!char.IsLetter(c) && c != '_') 
            return null;

        int i = start;
        while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
        return new Token(TokenKind.Word, input[start..i], start, i);
    }

    private static Token? ReadNumber(char c, string input, int start)
    {
        if (!char.IsDigit(c)) 
            return null;

        int i = start;
        while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] is '.' or '_')) i++;
        return new Token(TokenKind.Number, input[start..i], start, i);
    }

    private static Token? ReadString(string input, int start)
        => LiteralScanner.TryFindStringEnd(input, start, out int end)
            ? new Token(TokenKind.String, input[start..end], start, end)
            : null;

    private static Token? ReadChar(string input, int start)
        => LiteralScanner.TryFindCharEnd(input, start, out int end)
            ? new Token(TokenKind.Char, input[start..end], start, end)
            : null;

    private static Token ReadSymbolToken(string input, int start)
    {
        string sym = ReadSymbol(input, start);
        return new Token(TokenKind.Symbol, sym, start, start + sym.Length);
    }

    private static string ReadSymbol(string input, int i)
    {
        foreach (var op in _multiCharOps)
            if (i + op.Length <= input.Length && input.AsSpan(i, op.Length).SequenceEqual(op))
                return op;
        return input[i].ToString();
    }
}
