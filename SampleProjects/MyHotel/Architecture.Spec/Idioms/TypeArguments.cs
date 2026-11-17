using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 6k as an invariant over the syntax trees and what they bind to: a call
/// names no type argument the compiler infers. The call is bound again where it stands, without its type
/// arguments; when it reaches the same method with the same types, nullability included, they were
/// inferred. A call after `?.` is not looked at. One fact per project (Sources).</summary>
public class TypeArguments : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void ACallNamesNoTypeArgumentTheCompilerInfers(string project) =>
        Source.Projects[project].Nodes<GenericNameSyntax>().Has().None(Redundant);

    /// <summary>A called name whose type arguments say what the compiler infers.</summary>
    private static bool Redundant(SourceNode<GenericNameSyntax> name) =>
        CallOf(name.Syntax) is { } call
        && name.File.Model.GetSymbolInfo(call).Symbol is { } written
        && SymbolEqualityComparer.IncludeNullability.Equals(written, Inferred(name, call));

    /// <summary>The call a name is the called name of; null for a type's name and a method not
    /// called.</summary>
    private static InvocationExpressionSyntax? CallOf(GenericNameSyntax name) =>
        Called(name).Parent as InvocationExpressionSyntax;

    /// <summary>What a call of the name would call: the name itself, or the member access it ends.</summary>
    private static SyntaxNode Called(GenericNameSyntax name) =>
        name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;

    /// <summary>What the call reaches without the name's type arguments; null when the compiler cannot
    /// infer them.</summary>
    private static ISymbol? Inferred(SourceNode<GenericNameSyntax> name, InvocationExpressionSyntax call) =>
        name.File.Model.GetSpeculativeSymbolInfo(
            call.SpanStart,
            call.ReplaceNode(name.Syntax, SyntaxFactory.IdentifierName(name.Syntax.Identifier)),
            SpeculativeBindingOption.BindAsExpression).Symbol;
}
