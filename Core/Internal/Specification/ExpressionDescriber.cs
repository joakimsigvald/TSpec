using System.Runtime.CompilerServices;
using TSpec.Internal.Specification.ExpressionParsing.Describe;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;
using TSpec.Internal.Specification.ExpressionParsing.Parse;

namespace TSpec.Internal.Specification;

/// <summary>
/// Facade over the expression-parsing pipeline: preprocess the source
/// (<see cref="SourcePreprocessor.ToSingleLine"/>), parse it into an
/// <see cref="Expr"/> tree, and describe it. <see cref="Describe"/> is the
/// plain rendering; the other two add the context of a call or of the actual
/// under test, and fall back to it for anything they do not handle themselves.
/// </summary>
internal static class ExpressionDescriber
{
    public static string Describe(this string? expr)
        => string.IsNullOrWhiteSpace(expr) ? string.Empty
        : Describer.Value.Describe(Parser.Parse(expr.ToSingleLine()));

    public static string? DescribeCall(this string? expr, bool skipSubjectRef = false)
        => expr is null ? null
        : string.IsNullOrWhiteSpace(expr) ? string.Empty
        : new CallDescriber(skipSubjectRef).Describe(Parser.Parse(expr.ToSingleLine()));

    public static string DescribeActual(this string? expr, string? subject = null)
        => string.IsNullOrWhiteSpace(expr) ? string.Empty
        : new ActualDescriber(subject).Describe(Parser.Parse(expr.ToSingleLine()));

    /// Guard for Then/And subject expressions: a member access is only allowed
    /// on the result of a method call, so MethodCall().Property passes while
    /// value.Property and Property1.Property2 are trainwrecks. Only the
    /// top-level chain is inspected — call arguments (lambdas, constraints)
    /// never count.
    public static void AssertNoTrainwreck(this string? expr, [CallerMemberName] string? verb = null)
    {
        if (string.IsNullOrWhiteSpace(expr))
            return;

        var parsed = Parser.Parse(expr.ToSingleLine());
        if (!IsTrainwreck(parsed))
            return;

        var whole = parsed.ToSource();
        var root = RootOf(parsed).ToSource();
        throw new SetupFailed(
            $"No trainwrecks in {verb}: '{whole}' chains a member on its subject. "
            + $"Hand over the root and chain the rest after it: {verb}({root}){whole[root.Length..]}");
    }

    /// The innermost target the chain hangs off, which is what the subject should have been.
    private static Expr RootOf(Expr e) => e switch
    {
        Member m => IsTrainwreck(m.Target) ? RootOf(m.Target) : m.Target,
        Call c => RootOf(c.Target),
        _ => e,
    };

    private static bool IsTrainwreck(Expr e) => e switch
    {
        Member m => m.Target is not Call || IsTrainwreck(m.Target),
        Call c => IsTrainwreck(c.Target),
        _ => false,
    };
}
