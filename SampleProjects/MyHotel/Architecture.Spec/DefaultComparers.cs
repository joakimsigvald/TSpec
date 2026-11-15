using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 2f as an invariant over the syntax trees: a set is a collection
/// expression, and ToHashSet stays only for a comparer a collection expression cannot take. The default one
/// it can: EqualityComparer's Default, and for a string StringComparer.Ordinal. One fact per project
/// (Sources).</summary>
public class DefaultComparers : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoToHashSetPassesTheDefaultComparer(string project) =>
        Source.Projects[project].Nodes<ArgumentSyntax>().Has().None(argument => DefaultComparer(argument));

    private static bool DefaultComparer(SourceNode<ArgumentSyntax> argument) =>
        IsToHashSetsComparer(argument.Syntax) && IsTheDefault(argument.Syntax.Expression);

    /// <summary>The last argument of a ToHashSet call, whether it is called as an extension or not.</summary>
    private static bool IsToHashSetsComparer(ArgumentSyntax argument) =>
        argument.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax call } list
        && IsToHashSet(call)
        && list.Arguments[^1] == argument;

    private static bool IsToHashSet(InvocationExpressionSyntax call) =>
        call.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "ToHashSet" };

    private static bool IsTheDefault(ExpressionSyntax comparer) =>
        comparer is MemberAccessExpressionSyntax access
        && (Owner(access), access.Name.Identifier.Text)
            is ("StringComparer", "Ordinal") or ("EqualityComparer", "Default");

    /// <summary>The type a member is read from, without its namespace or type arguments.</summary>
    private static string Owner(MemberAccessExpressionSyntax access) =>
        access.Expression switch
        {
            MemberAccessExpressionSyntax qualified => qualified.Name.Identifier.Text,
            SimpleNameSyntax name => name.Identifier.Text,
            _ => "",
        };
}
