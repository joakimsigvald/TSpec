using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 2f as an invariant over the syntax trees and what they bind to: a set
/// is a collection expression, and ToHashSet stays only for a comparer a collection expression cannot take.
/// The default one it can: EqualityComparer's Default, and for a string StringComparer.Ordinal. One fact per
/// project (Sources).</summary>
public class DefaultComparers : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoToHashSetPassesTheDefaultComparer(string project) =>
        Source.Projects[project].Nodes<ArgumentSyntax>().Has().None(DefaultComparer);

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
        DefaultComparer(argument.Syntax, argument.File.Model);

    private static bool PassesTheDefault(SemanticModel snippet) =>
        Snippets.Nodes<ArgumentSyntax>(snippet).Any(argument => DefaultComparer(argument, snippet));

    private static bool DefaultComparer(ArgumentSyntax argument, SemanticModel model) =>
        IsToHashSetsComparer(argument, model) && IsTheDefault(argument.Expression, model);

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
    private static SemanticModel SetBy(string call) =>
        Snippets.Compiled($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using static System.StringComparer;
            using Strings = System.StringComparer;

            class C { object M(string[] words) => {{call}}; }
            """);
}
