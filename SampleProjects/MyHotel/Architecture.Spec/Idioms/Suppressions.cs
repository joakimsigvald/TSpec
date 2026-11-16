using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Idioms;

/// <summary>CODING-STANDARD-BACKEND.md 0d as an invariant over the syntax trees: every warning fails the
/// build, so a switch that turns one off in the source - `#pragma warning disable`, `[SuppressMessage]` or
/// `#nullable disable` - is the one way past every analyzer and guard, and our sources hold none. A rule that
/// does not fit is changed where review sees it: the standard and the .editorconfig. A project file's
/// `NoWarn` stays review's, since TSpec reads no project file. One fact per project (Sources).</summary>
public class Suppressions : TSpec.Spec
{
    // The ends of SuppressMessage and UnconditionalSuppressMessage, with or without a namespace or Attribute.
    private static readonly string[] SuppressionNames = ["SuppressMessage", "SuppressMessageAttribute"];

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void NoSourceSwitchesAWarningOff(string project) =>
        Source.Projects[project].Nodes<SyntaxNode>().Has().None(node => Suppressing(node));

    private static bool Suppressing(SourceNode<SyntaxNode> node) =>
        node.Syntax switch
        {
            PragmaWarningDirectiveTriviaSyntax pragma => Disables(pragma.DisableOrRestoreKeyword),
            NullableDirectiveTriviaSyntax nullable => Disables(nullable.SettingToken),
            AttributeSyntax attribute => Suppresses(attribute),
            _ => false,
        };

    private static bool Disables(SyntaxToken setting) => setting.IsKind(SyntaxKind.DisableKeyword);

    private static bool Suppresses(AttributeSyntax attribute) =>
        SuppressionNames.Any(name => attribute.Name.ToString().EndsWith(name, StringComparison.Ordinal));
}
