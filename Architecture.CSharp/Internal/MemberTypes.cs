using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The types a member's signature names, as the compiler resolves them: those of what it returns or holds,
/// and those of its parameters, each with the types written inside it.
/// </summary>
internal static class MemberTypes
{
    internal static IReadOnlyList<ITypeSymbol> GivenBy(DeclaredMember member)
        => member.Type is { } type ? Once(Within(member.File, type)) : [];

    internal static IReadOnlyList<ITypeSymbol> TakenBy(DeclaredMember member)
        => Once(member.Parameters.Select(parameter => parameter.Type).OfType<TypeSyntax>()
            .SelectMany(type => Within(member.File, type)));

    // Type arguments aside, as typeof(List<>) names List<int>.
    internal static bool IsOf(ITypeSymbol symbol, Type type)
        => symbol is INamedTypeSymbol named && MetadataNameOf(named.OriginalDefinition) == DefinitionOf(type).FullName;

    // The compiler resolves void to a type, but a void method gives nothing.
    private static IEnumerable<ITypeSymbol> Within(SourceFile file, TypeSyntax written)
        => file.Model.GetTypeInfo(written).Type is { SpecialType: not SpecialType.System_Void } type
            ? WrittenTypes.Of(type)
            : [];

    private static IReadOnlyList<ITypeSymbol> Once(IEnumerable<ITypeSymbol> types)
        => [.. types.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)];

    private static Type DefinitionOf(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

    private static string MetadataNameOf(INamedTypeSymbol type)
        => type.ContainingType is { } owner ? $"{MetadataNameOf(owner)}+{type.MetadataName}"
        : type.ContainingNamespace.IsGlobalNamespace ? type.MetadataName
        : $"{type.ContainingNamespace.ToDisplayString()}.{type.MetadataName}";
}
