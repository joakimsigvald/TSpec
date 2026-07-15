namespace TSpec.Internal.Specification.ExpressionParsing.Tokenize;

internal readonly record struct Token(TokenKind Kind, string Text, int Start, int End);