using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Exposure;

/// <summary>CODING-STANDARD-BACKEND.md 2e as an invariant over the source: IEnumerable&lt;T> is a sequence
/// read once. A method may take one and references that parameter exactly once; no member another class calls
/// returns one (IAsyncEnumerable&lt;T>, the streaming shape, is another type).</summary>
public class EnumerableMembers : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AMemberAnotherClassCallsReturnsNoEnumerable(string project) =>
        Source.Projects[project].Members.Has().None(member => ReturnsAnEnumerable(member));

    [Theory, MemberData(nameof(Projects))]
    public void AnEnumerableParameterIsReferencedExactlyOnce(string project) =>
        Source.Projects[project].Members.SelectMany(ParametersReadOtherThanOnce).Is().Empty();

    private static bool ReturnsAnEnumerable(DeclaredMember member) =>
        member.IsExposed && member.Gives(typeof(IEnumerable<>));

    /// <summary>"file:line method(parameter) referenced n times" for every IEnumerable parameter of a method
    /// or local function whose body names it other than once.</summary>
    private static IEnumerable<string> ParametersReadOtherThanOnce(DeclaredMember member) =>
        BodyOf(member.Syntax) is { } body ? Breaches(member, body) : [];

    private static IEnumerable<string> Breaches(DeclaredMember member, SyntaxNode body) =>
        member.Parameters.Where(HoldsAnEnumerable)
            .Select(parameter => Breach(member, parameter, body))
            .OfType<string>();

    private static string? Breach(DeclaredMember member, ParameterSyntax parameter, SyntaxNode body) =>
        Uses(body, parameter) is var uses and not 1
            ? $"{member}({parameter.Identifier.Text}) referenced {uses} times"
            : null;

    /// <summary>An abstract or interface method has no body and reads nothing; nor does a member of another
    /// kind.</summary>
    private static SyntaxNode? BodyOf(SyntaxNode declaration) =>
        declaration switch
        {
            MethodDeclarationSyntax m => (SyntaxNode?)m.Body ?? m.ExpressionBody,
            LocalFunctionStatementSyntax f => (SyntaxNode?)f.Body ?? f.ExpressionBody,
            _ => null,
        };

    private static bool HoldsAnEnumerable(ParameterSyntax parameter) =>
        parameter.Type is { } type && type.DescendantNodesAndSelf().Any(IsEnumerable);

    private static int Uses(SyntaxNode body, ParameterSyntax parameter) =>
        body.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Count(name => name.Identifier.Text == parameter.Identifier.Text);

    private static bool IsEnumerable(SyntaxNode node) =>
        node is GenericNameSyntax { Identifier.Text: "IEnumerable" };
}
