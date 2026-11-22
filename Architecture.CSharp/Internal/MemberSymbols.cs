using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// The compiler's symbol for a member, read at the token its name is written at.
/// </summary>
internal static class MemberSymbols
{
    // A record's positional parameter is its property too, and a primary constructor's token is its type's.
    internal static ISymbol Of(SemanticModel model, MemberDeclaration declaration)
        => model.GetDeclaredSymbol(declaration.At.Parent!) switch
        {
            IParameterSymbol { ContainingType: { } record } => PropertyOf(record, declaration.Name),
            INamedTypeSymbol type when declaration.Kind is MemberKind.Constructor => PrimaryConstructorOf(type, declaration.Syntax),
            var symbol => symbol!,
        };

    private static IPropertySymbol PropertyOf(INamedTypeSymbol record, string name)
        => record.GetMembers(name).OfType<IPropertySymbol>().First();

    private static IMethodSymbol PrimaryConstructorOf(INamedTypeSymbol type, SyntaxNode declaration)
        => type.InstanceConstructors.First(constructor
            => constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() == declaration));
}
