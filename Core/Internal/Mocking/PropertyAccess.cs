using System.Reflection;

namespace TSpec.Internal.Mocking;

/// Which calls reach a property, and where both its accessors meet.
internal static class PropertyAccess
{
    internal static bool IsRead(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("get_");

    internal static bool IsSet(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("set_");

    /// A property is addressed by its getter; one with none is addressed by its setter, as nothing reads it.
    internal static MethodInfo AddressedBy(MethodInfo setter) => GetterOf(setter) ?? setter;

    private static MethodInfo? GetterOf(MethodInfo setter)
        => setter.DeclaringType!
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(property => property.SetMethod == setter)?.GetMethod;
}
