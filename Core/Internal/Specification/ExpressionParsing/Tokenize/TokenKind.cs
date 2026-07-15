namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

internal enum TokenKind
{
    Word,
    Number,
    String,
    Char,
    Symbol,
    EndOfInput,
}