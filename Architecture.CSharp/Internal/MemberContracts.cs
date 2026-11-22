using Microsoft.CodeAnalysis;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// Whether a member's name and signature were set by a base type's member it overrides or an interface's
/// member it implements, explicitly or not, as the compiler sees it.
/// </summary>
internal static class MemberContracts
{
    internal static bool ImplementsOrOverrides(ISymbol member) => member.IsOverride || Implements(member);

    private static bool Implements(ISymbol member)
        => member.ContainingType is { } type
            && type.AllInterfaces.SelectMany(contract => contract.GetMembers())
                .Any(required => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(required), member));
}
