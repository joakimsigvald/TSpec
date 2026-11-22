using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Placement;

/// <summary>CODING-STANDARD-BACKEND.md 10e as an invariant over the syntax trees: a type's fields and
/// constants come first, then its constructors, then its other members by accessibility as written - public,
/// protected, internal, private - the public static ones before the public instance ones. Whether a caller
/// stands above what it calls is judgement, left to review, and a nested type has no place among the members.
/// A breach is counted.</summary>
public class MemberOrder : TSpec.Spec
{
    [Fact]
    public void FieldsComeFirstThenConstructorsThenMembersByAccessibility() =>
        Source.Projects.Has().All(
            project => project.Nodes<MemberDeclarationSyntax>().Has().None(StandsBelowOneItShouldPrecede));

    private static bool StandsBelowOneItShouldPrecede(SourceNode<MemberDeclarationSyntax> member) =>
        member.Syntax.Parent is TypeDeclarationSyntax type
        && IsOrdered(member.Syntax)
        && Above(member.Syntax, type).Any(above => PlaceOf(above, type) > PlaceOf(member.Syntax, type));

    private static IEnumerable<MemberDeclarationSyntax> Above(
        MemberDeclarationSyntax member,
        TypeDeclarationSyntax type) =>
        type.Members.TakeWhile(above => above != member).Where(IsOrdered);

    /// <summary>A nested type is a type of its own (10b, 10c), not a member to order.</summary>
    private static bool IsOrdered(MemberDeclarationSyntax member) =>
        member is not (BaseTypeDeclarationSyntax or DelegateDeclarationSyntax);

    private static Place PlaceOf(MemberDeclarationSyntax member, TypeDeclarationSyntax type) =>
        member switch
        {
            FieldDeclarationSyntax => Place.Field,
            ConstructorDeclarationSyntax => Place.Constructor,
            _ when IsPublic(member, type) => PublicPlaceOf(member),
            _ when member.Modifiers.Any(SyntaxKind.ProtectedKeyword) => Place.Protected,
            _ when member.Modifiers.Any(SyntaxKind.InternalKeyword) => Place.Internal,
            _ => Place.Private,
        };

    private static Place PublicPlaceOf(MemberDeclarationSyntax member) =>
        member.Modifiers.Any(SyntaxKind.StaticKeyword) ? Place.PublicStatic : Place.PublicInstance;

    /// <summary>Public as written: an explicit interface implementation is, and so is an interface's own
    /// member that names no accessibility.</summary>
    private static bool IsPublic(MemberDeclarationSyntax member, TypeDeclarationSyntax type) =>
        member.Modifiers.Any(SyntaxKind.PublicKeyword)
        || member is BasePropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null }
        || member is MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null }
        || (type is InterfaceDeclarationSyntax && !NamesAnAccessibility(member));

    private static bool NamesAnAccessibility(MemberDeclarationSyntax member) =>
        member.Modifiers.Any(SyntaxKind.ProtectedKeyword)
        || member.Modifiers.Any(SyntaxKind.InternalKeyword)
        || member.Modifiers.Any(SyntaxKind.PrivateKeyword);

    /// <summary>The places of 10e, in their order; protected internal and private protected are
    /// protected.</summary>
    private enum Place
    {
        Field,
        Constructor,
        PublicStatic,
        PublicInstance,
        Protected,
        Internal,
        Private,
    }
}
