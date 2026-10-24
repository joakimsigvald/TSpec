using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A type with the types written inside it: its type arguments, a tuple's elements among them, and an array's
/// or a pointer's element. A named type's members are its own, so they are not inside it.
/// </summary>
internal static class WrittenTypes
{
    internal static IEnumerable<ITypeSymbol> Of(ITypeSymbol type)
        => type switch
        {
            INamedTypeSymbol named => [named, .. named.TypeArguments.SelectMany(Of)],
            IArrayTypeSymbol array => [array, .. Of(array.ElementType)],
            IPointerTypeSymbol pointer => [pointer, .. Of(pointer.PointedAtType)],
            IFunctionPointerTypeSymbol pointer => [pointer, .. SignatureOf(pointer.Signature).SelectMany(Of)],
            _ => [type],
        };

    private static IEnumerable<ITypeSymbol> SignatureOf(IMethodSymbol signature)
        => [signature.ReturnType, .. signature.Parameters.Select(parameter => parameter.Type)];
}
