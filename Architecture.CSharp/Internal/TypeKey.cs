using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A type as both its declaration and a compilation's symbol for it name it: its path runs through the
/// types it is nested in, as metadata names them: "Outer`1+Inner".
/// </summary>
internal readonly record struct TypeKey(string Assembly, string Namespace, string Path)
{
    internal static TypeKey Of(INamedTypeSymbol type)
        => new(type.ContainingAssembly?.Name ?? "", NamespaceOf(type.ContainingNamespace), PathOf(type));

    internal static TypeKey Of(DeclaredType type, string assembly) => new(assembly, type.Namespace, PathOf(type));

    internal bool IsNested => Path.Contains('+');

    internal TypeKey Owner => this with { Path = Path[..Path.LastIndexOf('+')] };

    private static string PathOf(INamedTypeSymbol type)
        => type.ContainingType is { } owner ? $"{PathOf(owner)}+{type.MetadataName}" : type.MetadataName;

    private static string PathOf(DeclaredType type)
        => type.Owner is { } owner ? $"{PathOf(owner)}+{MetadataName(type)}" : MetadataName(type);

    private static string MetadataName(DeclaredType type) => type.Arity == 0 ? type.Name : $"{type.Name}`{type.Arity}";

    private static string NamespaceOf(INamespaceSymbol? @namespace)
        => @namespace is null || @namespace.IsGlobalNamespace ? "" : @namespace.ToDisplayString();
}
