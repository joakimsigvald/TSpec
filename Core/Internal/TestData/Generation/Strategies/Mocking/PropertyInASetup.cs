using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// A mock keeps nothing set on it, and is set up only after the values are arranged, so a setup lambda
/// setting its property would be lost, and one reading it before then would get the unarranged answer.
/// </summary>
internal static class PropertyInASetup
{
    internal static bool IsSet(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("set_");

    internal static bool IsRead(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("get_");

    internal static SetupFailed SetRefusal(Type mockedType, MethodInfo method, object?[] arguments)
    {
        var service = mockedType.Alias();
        var access = AccessOf(method, arguments[..^1]);
        var value = LiteralOf(arguments[^1]);
        return new(
            $"{service} is a mock and ignores {access.TrimStart('.')} = {value}. "
            + $"Arrange it with Given<{service}>().That(_ => _{access}).Returns(() => {value})");
    }

    internal static SetupFailed ReadRefusal(Type mockedType, MethodInfo method, object?[] arguments)
    {
        var service = mockedType.Alias();
        var access = AccessOf(method, arguments);
        var shared = $"The<{method.ReturnType.Alias()}>()";
        return new(
            $"{service}{access} is read before {service} is set up. Share a value instead: "
            + $"Given<{service}>().That(_ => _{access}).Returns(() => {shared}), and use {shared} in the setup");
    }

    private static string AccessOf(MethodInfo method, object?[] indices)
        => indices.Length == 0
            ? $".{method.Name[4..]}"
            : $"[{string.Join(", ", indices.Select(LiteralOf))}]";

    /// Written out only where the value's text is also the code that makes it.
    private static string LiteralOf(object? value)
        => value is null or string or bool or int or long or double ? value.FormatValue() : "…";
}
