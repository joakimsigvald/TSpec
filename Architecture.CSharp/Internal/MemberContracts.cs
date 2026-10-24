using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// Whether a member's name and signature were set by a base type's member it overrides or an interface's
/// member it implements, explicitly or not, as the compiler sees it.
/// </summary>
internal static class MemberContracts
{
    internal static bool ImplementsOrOverrides(SourceFile file, MemberDeclaration declaration)
        => SymbolOf(file.Model, declaration) is { } member && (member.IsOverride || Implements(member));

    // A record's positional parameter is its property too, and the property is what an interface names; a
    // primary constructor's token, and a nested delegate's, are a type's.
    private static ISymbol? SymbolOf(SemanticModel model, MemberDeclaration declaration)
        => model.GetDeclaredSymbol(declaration.At.Parent!) switch
        {
            IParameterSymbol { ContainingType: { } record } => PropertyOf(record, declaration.Name),
            INamedTypeSymbol => null,
            var symbol => symbol,
        };

    private static IPropertySymbol? PropertyOf(INamedTypeSymbol record, string name)
        => record.GetMembers(name).OfType<IPropertySymbol>().FirstOrDefault();

    private static bool Implements(ISymbol member)
        => member.ContainingType is { } type
            && type.AllInterfaces.SelectMany(contract => contract.GetMembers())
                .Any(required => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(required), member));
}
