using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        Source.Projects[project].Members.Has().None(ReturnsAnEnumerable);

    [Theory, MemberData(nameof(Projects))]
    public void AnEnumerableParameterIsReferencedExactlyOnce(string project) =>
        Source.Projects[project].Members.SelectMany(ParametersReadOtherThanOnce).Is().Empty();

    [Theory]
    [InlineData("int Count(IEnumerable<int> rows) => rows.Count();", 1)]
    [InlineData("int Twice(IEnumerable<int> rows) => rows.Count() + rows.Sum();", 2)]
    [InlineData("int Max(IEnumerable<int> rows) => rows.Max(row => new[] { row }.Max(rows => rows));", 1)]
    public void TheReferencesToAParameterAreTheNamesThatResolveToIt(string method, int references) =>
        ReferencesIn(Method(method)).Is(references);

    [Theory]
    [InlineData("int Count(IEnumerable<int> rows) => 0;")]
    [InlineData("int Count(Rows rows) => 0;")]
    [InlineData("int Count(Func<IEnumerable<int>> rows) => 0;")]
    [InlineData("int Count(IEnumerable<int>[] rows) => 0;")]
    public void AParameterHoldsAnEnumerableHoweverItsTypeIsWritten(string method) =>
        HoldsAnEnumerable(Method(method)).Is().True();

    [Theory]
    [InlineData("int Count(IReadOnlyList<int> rows) => 0;")]
    [InlineData("int Count(string rows) => 0;")]
    public void ATypeThatOnlyImplementsOneHoldsNone(string method) =>
        HoldsAnEnumerable(Method(method)).Is().False();

    private static bool ReturnsAnEnumerable(DeclaredMember member) =>
        member.IsExposed && member.Gives(typeof(IEnumerable<>));

    /// <summary>"file:line method(parameter) referenced n times" for every IEnumerable parameter of a method
    /// or local function whose body names it other than once.</summary>
    private static IEnumerable<string> ParametersReadOtherThanOnce(DeclaredMember member) =>
        BodyOf(member.Syntax) is { } body ? Breaches(member, body) : [];

    private static IEnumerable<string> Breaches(DeclaredMember member, SyntaxNode body) =>
        ParametersOf(member).Where(parameter => Holds(parameter.Type))
            .Select(parameter => Breach(member, parameter, body))
            .OfType<string>();

    private static string? Breach(DeclaredMember member, IParameterSymbol parameter, SyntaxNode body) =>
        Uses(body, parameter, member.File.Model) is var uses and not 1
            ? $"{member}({parameter.Name}) referenced {uses} times"
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

    private static IReadOnlyList<IParameterSymbol> ParametersOf(DeclaredMember member) =>
        member.Symbol is IMethodSymbol method ? method.Parameters : [];

    /// <summary>An IEnumerable&lt;T> itself, or a type that has one among its type arguments or as its
    /// elements; a type that only implements one holds none.</summary>
    private static bool Holds(ITypeSymbol type) =>
        type switch
        {
            IArrayTypeSymbol array => Holds(array.ElementType),
            INamedTypeSymbol named => IsEnumerable(named) || named.TypeArguments.Any(Holds),
            _ => false,
        };

    private static bool IsEnumerable(INamedTypeSymbol type) =>
        type.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T;

    private static int Uses(SyntaxNode body, IParameterSymbol parameter, SemanticModel model) =>
        body.DescendantNodes().OfType<IdentifierNameSyntax>().Count(name => Names(name, parameter, model));

    /// <summary>Whether the name resolves to the parameter: a lambda's own parameter of that name does
    /// not.</summary>
    private static bool Names(IdentifierNameSyntax name, IParameterSymbol parameter, SemanticModel model) =>
        SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name).Symbol, parameter);

    /// <summary>The method in a class of its own in a snippet, where `Rows` is an alias of
    /// IEnumerable&lt;int>.</summary>
    private static DeclaredMember Method(string method) =>
        Source.Snippet($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Rows = System.Collections.Generic.IEnumerable<int>;

            class C { {{method}} }
            """).Members.Single();

    /// <summary>The references to the method's first parameter.</summary>
    private static int ReferencesIn(DeclaredMember method) =>
        Uses(BodyOf(method.Syntax)!, ParametersOf(method)[0], method.File.Model);

    private static bool HoldsAnEnumerable(DeclaredMember method) => Holds(ParametersOf(method)[0].Type);
}
