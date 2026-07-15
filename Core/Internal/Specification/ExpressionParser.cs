using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using TSpec.Internal.Specification.ExpressionParsing.Describe;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;
using TSpec.Internal.Specification.ExpressionParsing.Parse;
using TSpec.Internal.Specification.ExpressionParsing.Tokenize;

namespace TSpec.Internal.Specification;

internal static partial class ExpressionParser
{
    private static readonly char[] _lineBreakCues = ['.', '(', '['];

    public static string ParseValue(this string? expr)
        => string.IsNullOrWhiteSpace(expr) ? string.Empty
        : Describer.Value.Describe(Parser.Parse(expr.ToSingleLine()));

    public static string? ParseCall(this string? expr, bool skipSubjectRef = false)
        => expr is null ? null
        : string.IsNullOrWhiteSpace(expr) ? string.Empty
        : new CallDescriber(skipSubjectRef).Describe(Parser.Parse(expr.ToSingleLine()));

    public static string ParseActual(this string? expr, string? subject = null)
        => string.IsNullOrWhiteSpace(expr) ? string.Empty
        : new ActualDescriber(subject).Describe(Parser.Parse(expr.ToSingleLine()));

    /// Guard for Then/And subject expressions: a member access is only allowed
    /// on the result of a method call, so MethodCall().Property passes while
    /// value.Property and Property1.Property2 are trainwrecks. Only the
    /// top-level chain is inspected — call arguments (lambdas, constraints)
    /// never count.
    public static void AssertNoTrainwreck(this string? expr)
    {
        if (!string.IsNullOrWhiteSpace(expr) && IsTrainwreck(Parser.Parse(expr.ToSingleLine())))
            throw new SetupFailed("No trainwrecks in Then/And! Chain additional properties/method calls outside of the subject expression");
    }

    private static bool IsTrainwreck(Expr e) => e switch
    {
        Member m => m.Target is not Call || IsTrainwreck(m.Target),
        Call c => IsTrainwreck(c.Target),
        _ => false,
    };

    [return: NotNullIfNotNull(nameof(str))]
    public static string? ToSingleLine(this string? str)
        => string.IsNullOrEmpty(str) ? str : MergeLines([.. ToLines(str)]);

    private static IEnumerable<string> ToLines(string str)
        => LineBreakRegex()
        .Split(str)
        .Select(StripLineComment)
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim());

    private static string MergeLines(this string[] lines)
    {
        StringBuilder sb = new();
        bool addSpace = false;
        foreach (var line in lines)
        {
            if (addSpace && !line.StartsWith('.'))
                sb.Append(' ');
            sb.Append(line);
            addSpace = !_lineBreakCues.Contains(line[^1]);
        }
        return sb.ToString();
    }

    private static string StripLineComment(string line)
    {
        for (var i = 0; i < line.Length; i = Advance(line, i))
            if (line[i..].StartsWith("//"))
                return line[..i];
        return line;
    }

    /// Skips past any string or char literal at <paramref name="i"/>. If
    /// there's no literal here, advances one character.
    private static int Advance(string line, int i)
    {
        int charEnd = LiteralScanner.TryFindCharEnd(line, i);
        if (charEnd >= 0) return charEnd;
        int strEnd = LiteralScanner.TryFindStringEnd(line, i);
        if (strEnd >= 0) return strEnd;
        return i + 1;
    }

    [GeneratedRegex(@"(\r|\n)+")]
    private static partial Regex LineBreakRegex();
}