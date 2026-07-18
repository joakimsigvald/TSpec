using TSpec.Internal.Specification.ExpressionParsing.Describe;
using TSpec.Internal.Specification.ExpressionParsing.Expressions;
using TSpec.Internal.Specification.ExpressionParsing.Parse;

namespace TSpec.Internal.Specification;

/// <summary>
/// Facade over the expression-parsing pipeline: preprocess the source
/// (<see cref="SourcePreprocessor.ToSingleLine"/>), parse it into an
/// <see cref="Expr"/> tree, and describe the tree in the mode the caller
/// needs (value, call, or actual).
/// </summary>
internal static class ExpressionParser
{
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
}
