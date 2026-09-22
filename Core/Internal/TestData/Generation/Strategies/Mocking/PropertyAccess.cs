using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// Which calls reach a property, and where both its accessors meet.
internal static class PropertyAccess
{
    internal static bool IsRead(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("get_");

    internal static bool IsSet(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("set_");

    /// The property both accessors belong to, taken where it is declared so an override meets it there.
    internal static (Type, string) PropertyOf(MethodInfo accessor)
    {
        var declared = accessor.GetBaseDefinition();
        return (declared.DeclaringType!, declared.Name[4..]);
    }
}
