using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 0d as an invariant over the syntax trees and what they bind to: every
/// warning fails the build, so a switch that turns one off in the source - `#pragma warning disable`,
/// `[SuppressMessage]` or `#nullable disable` - is the one way past every analyzer and guard, and our sources
/// hold none. A rule that does not fit is changed where review sees it: the standard and the .editorconfig. A
/// project file's `NoWarn` stays review's, since TSpec reads no project file. One fact per project
/// (Sources).</summary>
public class Suppressions : TSpec.Spec
{
    private static readonly string[] SuppressionTypes =
    [
        "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
        "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute",
    ];

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoSourceSwitchesAWarningOff(string project) =>
        Source.Projects[project].Nodes<SyntaxNode>().Has().None(Suppressing);

    [Theory]
    [InlineData("""[SuppressMessage("Style", "IDE0001")]""")]
    [InlineData("""[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Style", "IDE0001")]""")]
    [InlineData("""[UnconditionalSuppressMessage("Trimming", "IL2026")]""")]
    [InlineData("""[Quiet("Style", "IDE0001")]""")]
    public void AnAttributeSwitchesAWarningOff(string attribute) =>
        Suppresses(OnAClass(attribute)).Is().True();

    [Theory]
    [InlineData("[Obsolete]")]
    [InlineData("""[NotASuppressMessage("Style")]""")]
    public void AnotherAttributeDoesNot(string attribute) => Suppresses(OnAClass(attribute)).Is().False();

    private static bool Suppressing(SourceNode<SyntaxNode> node) =>
        node.Syntax switch
        {
            PragmaWarningDirectiveTriviaSyntax pragma => Disables(pragma.DisableOrRestoreKeyword),
            NullableDirectiveTriviaSyntax nullable => Disables(nullable.SettingToken),
            AttributeSyntax attribute => Suppresses(attribute, node.File.Model),
            _ => false,
        };

    private static bool Disables(SyntaxToken setting) => setting.IsKind(SyntaxKind.DisableKeyword);
    private static bool Suppresses(SourceFile snippet) => snippet.Nodes<SyntaxNode>().Any(Suppressing);

    private static bool Suppresses(AttributeSyntax attribute, SemanticModel model) =>
        model.GetSymbolInfo(attribute).Symbol?.ContainingType.ToDisplayString() is { } type
        && SuppressionTypes.Contains(type);

    /// <summary>A snippet with the attribute on a class, where `Quiet` is an alias of SuppressMessage and
    /// NotASuppressMessage an attribute of our own.</summary>
    private static SourceFile OnAClass(string attribute) =>
        Source.Snippet($$"""
            using System;
            using System.Diagnostics.CodeAnalysis;
            using Quiet = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

            {{attribute}}
            sealed class C { }

            sealed class NotASuppressMessage(string category) : Attribute
            {
                public string Category => category;
            }
            """);
}
