using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD.md 10e as an invariant over the syntax trees: a type's fields and constants come
/// first, then its constructors, then its other members by accessibility as written - public, protected,
/// internal, private - the public static ones before the public instance ones. Whether a caller stands above
/// what it calls is judgement, left to review, and a nested type has no place among the members. One fact
/// per project (Sources); a breach is counted.</summary>
public class MemberOrder : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void FieldsComeFirstThenConstructorsThenMembersByAccessibility(string project) =>
        Breach(project).Is().Null();

    private static string? Breach(string project) =>
        Misplaced(project) is [var first, ..] and var misplaced
            ? $"{misplaced.Count} members stand below one they should precede, the first at {first}"
            : null;

    /// <summary>"file:line" for each member of the project standing below one it should precede.</summary>
    private static IReadOnlyList<string> Misplaced(string project) =>
        [.. Source.Projects[project].Files.SelectMany(MisplacedIn)];

    private static IEnumerable<string> MisplacedIn(SourceFile file) =>
        file.Root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .SelectMany(type => Below(PlacedIn(type)))
            .Select(member => $"{file}:{file.Line(member)}");

    /// <summary>The members with a member above them whose place comes later.</summary>
    private static IEnumerable<MemberDeclarationSyntax> Below(IReadOnlyList<Placed> members) =>
        members.Where((member, at) => members.Take(at).Any(above => above.Place > member.Place))
            .Select(member => member.Syntax);

    private static IReadOnlyList<Placed> PlacedIn(TypeDeclarationSyntax type) =>
        [.. type.Members.Where(IsOrdered).Select(member => new Placed(member, PlaceOf(member, type)))];

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

    private sealed record Placed(MemberDeclarationSyntax Syntax, Place Place);
}
