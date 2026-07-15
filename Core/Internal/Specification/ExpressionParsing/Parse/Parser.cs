using TSpec.Internal.Specification.ExpressionParsing.Tokenize;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;

namespace TSpec.Internal.Specification.ExpressionParsing.Parse;

/// <summary>
/// Entry point for parsing a single C# expression into an <see cref="Expr"/>
/// tree. The grammar is split across one rule class per precedence level:
/// <see cref="LambdaRule"/> → <see cref="AssignmentRule"/> →
/// <see cref="ConditionalRule"/> → <see cref="BinaryRule"/> →
/// <see cref="UnaryRule"/> → <see cref="PostfixRule"/> →
/// <see cref="PrimaryRule"/>. Anything unparseable becomes
/// <see cref="Unknown"/>(<c>raw text</c>) so the surrounding describer can
/// still render the original source verbatim.
/// </summary>
internal static class Parser
{
    public static Expr Parse(string source)
    {
        if (string.IsNullOrEmpty(source)) 
            return new Unknown(source ?? string.Empty);

        var ts = new TokenStream(source);
        try
        {
            var expr = LambdaRule.Parse(ts);
            return ts.Peek().Kind == TokenKind.EndOfInput ? expr : new Unknown(source.Trim());
        }
        catch
        {
            return new Unknown(source.Trim());
        }
    }
}