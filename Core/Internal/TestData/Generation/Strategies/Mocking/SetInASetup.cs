using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// A mock keeps nothing set on it, so a set made in a setup lambda would be lost while the specification states it.
internal static class SetInASetup
{
    internal static bool IsPropertySet(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("set_");

    internal static SetupFailed Refusal(Type mockedType, MethodInfo method, object?[] arguments)
    {
        var service = mockedType.Alias();
        var access = AccessOf(method, arguments[..^1]);
        var value = LiteralOf(arguments[^1]);
        return new(
            $"{service} is a mock and ignores {access.TrimStart('.')} = {value}. "
            + $"Arrange it with Given<{service}>().That(_ => _{access}).Returns(() => {value})");
    }

    private static string AccessOf(MethodInfo method, object?[] indices)
        => indices.Length == 0
            ? $".{method.Name[4..]}"
            : $"[{string.Join(", ", indices.Select(LiteralOf))}]";

    /// Written out only where the value's text is also the code that makes it.
    private static string LiteralOf(object? value)
        => value is null or string or bool or int or long or double ? value.FormatValue() : "…";
}
