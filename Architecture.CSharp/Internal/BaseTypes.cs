using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A declared type's base types as the compiler resolves them, the nearest first.
/// </summary>
internal static class BaseTypes
{
    internal static IReadOnlyList<INamedTypeSymbol> Of(DeclaredType type)
        => type.Declarations is [var declaration, ..]
            && declaration.File.Model.GetDeclaredSymbol(declaration.Syntax) is INamedTypeSymbol symbol
            ? [.. Above(symbol)]
            : [];

    private static IEnumerable<INamedTypeSymbol> Above(INamedTypeSymbol type)
        => type.BaseType is { } baseType ? [baseType, .. Above(baseType)] : [];
}
