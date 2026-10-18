using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A type as both its declaration and a compilation's symbol for it name it.
/// </summary>
internal readonly record struct TypeKey(string Assembly, string Namespace, string Name, int Arity)
{
    internal static TypeKey Of(INamedTypeSymbol type)
        => new(type.ContainingAssembly?.Name ?? "", NamespaceOf(type.ContainingNamespace), type.Name, type.Arity);

    private static string NamespaceOf(INamespaceSymbol? @namespace)
        => @namespace is null || @namespace.IsGlobalNamespace ? "" : @namespace.ToDisplayString();
}
