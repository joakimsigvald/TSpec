using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// A name counts calls to a method. A property's name does not say which accessor was meant.
internal static class VerificationByName
{
    private const BindingFlags AnyInstanceMember = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    internal static void AssertNamesAMethod(Type service, string name)
    {
        if (Declares(service, MemberTypes.Method, name))
            return;

        throw new SetupFailed(RefusalOf(service, name));
    }

    private static string RefusalOf(Type service, string name)
        => Declares(service, MemberTypes.Property, name) ? NotByName(service, name, "property")
        : Declares(service, MemberTypes.Field, name) ? NotByName(service, name, "field")
        : $"{service.Alias()} has no method {name}";

    private static string NotByName(Type service, string name, string kind)
        => $"{service.Alias()}.{name} is a {kind}; fields and properties cannot be verified by name";

    private static bool Declares(Type service, MemberTypes kind, string name)
        => TypesOf(service).Any(type => type.GetMember(name, kind, AnyInstanceMember).Length > 0);

    /// An interface's members include those it inherits, which reflection lists only on the interfaces themselves.
    private static Type[] TypesOf(Type service)
        => service.IsInterface ? [service, .. service.GetInterfaces()] : [service];
}
