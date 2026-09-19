using System.Reflection;
using TSpec.Internal.Pipelines;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// What a setup lambda may not do to a mock, since the mock would not answer as the specification states.
internal sealed class SetupGuard(IPipelinePhase phase, SetupLambda setupLambda)
{
    internal void Check(Type mockedType, MethodInfo method, object?[] arguments)
    {
        if (!setupLambda.IsRunning)
            return;

        if (IsSet(method))
            throw SetRefusal(mockedType, method, arguments);
        if (phase.Current < Phase.Mock && IsRead(method))
            throw ReadRefusal(mockedType, method, arguments);
    }

    private static bool IsSet(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("set_");

    private static bool IsRead(MethodInfo method)
        => method.IsSpecialName && method.Name.StartsWith("get_");

    /// A mock keeps nothing set on it, so the set would be lost while the specification states it.
    private static SetupFailed SetRefusal(Type mockedType, MethodInfo method, object?[] arguments)
    {
        var service = mockedType.Alias();
        var access = AccessOf(method, arguments[..^1]);
        var value = LiteralOf(arguments[^1]);
        return new(
            $"{service} is a mock and ignores {access.TrimStart('.')} = {value}. "
            + $"Arrange it with Given<{service}>().That(_ => _{access}).Returns(() => {value})");
    }

    /// Mocks are set up after the values are arranged, so a read before then gets the unarranged answer.
    private static SetupFailed ReadRefusal(Type mockedType, MethodInfo method, object?[] arguments)
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
