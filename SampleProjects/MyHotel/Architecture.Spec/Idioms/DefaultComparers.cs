using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 2f as an invariant over the syntax trees and what they bind to: a set
/// is a collection expression, and ToHashSet stays only for a comparer a collection expression cannot take.
/// The default one it can: EqualityComparer's Default, and for a string StringComparer.Ordinal.</summary>
public class DefaultComparers : TSpec.Spec
{
    [Fact]
    public void NoToHashSetPassesTheDefaultComparer() =>
        Source.Projects.Has().All(project => project.Nodes<ArgumentSyntax>().Has().None(DefaultComparer));

    [Theory]
    [InlineData("words.ToHashSet(StringComparer.Ordinal)")]
    [InlineData("words.ToHashSet(System.StringComparer.Ordinal)")]
    [InlineData("words.ToHashSet(EqualityComparer<string>.Default)")]
    [InlineData("Enumerable.ToHashSet(words, StringComparer.Ordinal)")]
    [InlineData("words.ToHashSet(Strings.Ordinal)")]
    [InlineData("words.ToHashSet(Ordinal)")]
    public void ToHashSetPassesTheDefaultComparer(string call) => PassesTheDefault(SetBy(call)).Is().True();

    [Theory]
    [InlineData("words.ToHashSet()")]
    [InlineData("words.ToHashSet(StringComparer.OrdinalIgnoreCase)")]
    [InlineData("new HashSet<string>(words, StringComparer.Ordinal)")]
    public void ToHashSetPassesAnotherOrNone(string call) => PassesTheDefault(SetBy(call)).Is().False();

    private static bool DefaultComparer(SourceNode<ArgumentSyntax> argument) =>
        IsToHashSetsComparer(argument.Syntax, argument.File.Model)
        && IsTheDefault(argument.Syntax.Expression, argument.File.Model);

    private static bool PassesTheDefault(SourceFile snippet) =>
        snippet.Nodes<ArgumentSyntax>().Any(DefaultComparer);

    /// <summary>The last argument of a ToHashSet call, whether it is called as an extension or not.</summary>
    private static bool IsToHashSetsComparer(ArgumentSyntax argument, SemanticModel model) =>
        argument.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax call } list
        && IsToHashSet(call, model)
        && list.Arguments[^1] == argument;

    private static bool IsToHashSet(InvocationExpressionSyntax call, SemanticModel model) =>
        model.GetSymbolInfo(call).Symbol is IMethodSymbol { Name: "ToHashSet" } method
        && method.ContainingType.ToDisplayString() == "System.Linq.Enumerable";

    private static bool IsTheDefault(ExpressionSyntax comparer, SemanticModel model) =>
        model.GetSymbolInfo(comparer).Symbol?.OriginalDefinition.ToDisplayString()
            is "System.StringComparer.Ordinal" or "System.Collections.Generic.EqualityComparer<T>.Default";

    /// <summary>A snippet making a set of `words` by the call, where `Strings` is an alias of StringComparer
    /// and its members are imported too.</summary>
    private static SourceFile SetBy(string call) =>
        Source.Snippet($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using static System.StringComparer;
            using Strings = System.StringComparer;

            class C { object M(string[] words) => {{call}}; }
            """);
}
