using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// What the syntax says of one member, its name at the token it is written at.
/// </summary>
internal sealed record MemberDeclaration(
    SyntaxNode Syntax, MemberKind Kind, SyntaxToken At, TypeSyntax? Type, IReadOnlyList<ParameterSyntax> Parameters,
    SyntaxTokenList Modifiers, IEnumerable<AttributeListSyntax> AttributeLists, Accessibility Accessibility)
{
    public string Name { get; init; } = At.Text;

    // What the member's size is measured over: a primary constructor's declaration is its whole type.
    public SyntaxNode Extent { get; init; } = Syntax;
}
