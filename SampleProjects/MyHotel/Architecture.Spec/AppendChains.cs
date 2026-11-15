using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 6j as an invariant over the syntax trees and what they bind to: a
/// chain of `Append` and `AppendLine` calls on a StringBuilder holds at most one fixed text - a constant, or
/// the empty line of `AppendLine()`. Two are a text over several lines, which is one raw string literal. A
/// chain is looked at, not appends in statements of their own. One fact per project (Sources).</summary>
public class AppendChains : TSpec.Spec
{
    private const int FixedTexts = 1;
    private const string Builder = "System.Text.StringBuilder";

    private static readonly string[] Appends = ["Append", "AppendLine"];

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AChainOfAppendsHoldsAtMostOneFixedText(string project) =>
        Breach(project).Is().Null();

    private static string? Breach(string project) =>
        Sites(project) is [var first, ..] and var sites
            ? $"{sites.Count} chains append a text over several lines, the first at {first}"
            : null;

    /// <summary>"file:line" for every chain of the project that holds more than one fixed text.</summary>
    private static IReadOnlyList<string> Sites(string project) =>
        [.. Source.Projects[project].Nodes<InvocationExpressionSyntax>()
            .Where(EndsATextInParts).Select(Reported)];

    /// <summary>The last call of a chain of appends that holds more than one fixed text.</summary>
    private static bool EndsATextInParts(SourceNode<InvocationExpressionSyntax> call) =>
        IsAnAppend(call.File, call.Syntax)
        && !IsAppendedTo(call)
        && ChainEndingAt(call.File, call.Syntax).Count(append => IsFixed(call.File, append)) > FixedTexts;

    /// <summary>Whether the call is what the chain's next append is called on.</summary>
    private static bool IsAppendedTo(SourceNode<InvocationExpressionSyntax> call) =>
        call.Syntax.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax next }
        && IsAnAppend(call.File, next);

    /// <summary>The appends of a chain in the order they are written, up to the one given.</summary>
    private static IEnumerable<InvocationExpressionSyntax> ChainEndingAt(
        SourceFile file, InvocationExpressionSyntax last) =>
        CalledOn(last) is { } before && IsAnAppend(file, before)
            ? ChainEndingAt(file, before).Append(last)
            : [last];

    /// <summary>The call the given one is called on; null when it is called on anything else.</summary>
    private static InvocationExpressionSyntax? CalledOn(InvocationExpressionSyntax call) =>
        (call.Expression as MemberAccessExpressionSyntax)?.Expression as InvocationExpressionSyntax;

    /// <summary>A call of a StringBuilder's `Append` or `AppendLine`.</summary>
    private static bool IsAnAppend(SourceFile file, InvocationExpressionSyntax call) =>
        call.Expression is MemberAccessExpressionSyntax access
        && Appends.Contains(access.Name.Identifier.Text)
        && file.Model.GetSymbolInfo(call).Symbol is IMethodSymbol method
        && method.ContainingType.ToDisplayString() == Builder;

    /// <summary>An append of a text the code fixes: a constant, or nothing but the line's end.</summary>
    private static bool IsFixed(SourceFile file, InvocationExpressionSyntax append) =>
        append.ArgumentList.Arguments is not [var first, ..]
        || file.Model.GetConstantValue(first.Expression).HasValue;

    private static string Reported(SourceNode<InvocationExpressionSyntax> call) =>
        $"{call.File}:{call.File.Line(call.Syntax)}";
}
