using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// Where a mock keeps what it answered: the member called and the arguments it was called with. A
/// property's set leads to its getter, so the set and the read of one property share an address.
/// </summary>
internal sealed record CallAddress(MethodInfo Member, object?[] Arguments)
{
    internal static CallAddress Of(MethodInfo method, object?[] arguments)
        => PropertyAccess.IsSet(method)
            ? new(PropertyAccess.AddressedBy(method), arguments[..^1])
            : new(method, arguments);
}
