using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// The calls a mock received, in order, each written as the call would be: what a failed
/// verification shows next to the call it expected.
/// </summary>
internal static class ReceivedCalls
{
    internal static string Of(MockHandle mock)
    {
        var mockName = mock.MockedType.Alias();
        var calls = mock.CountedInvocations;
        if (calls.Count == 0)
            return $"{mockName} received no calls";

        return string.Join(
            Environment.NewLine,
            [$"{mockName} received:", .. calls.Select(call => $"  {Describe(mockName, call)}")]);
    }

    /// A property reads as the property, a delegate's Invoke as the delegate; any other member by its method's name.
    internal static string Describe(string mockName, MockInvocation call)
    {
        var method = call.Method;
        var arguments = call.Arguments.Select(argument => argument.FormatValue()).ToArray();
        if (IsDelegateInvoke(method))
            return $"{mockName}({string.Join(", ", arguments)})";
        if (method.IsSpecialName && method.Name.StartsWith("get_") && arguments.Length == 0)
            return $"{mockName}.{method.Name[4..]}";
        if (method.IsSpecialName && method.Name.StartsWith("set_") && arguments.Length == 1)
            return $"{mockName}.{method.Name[4..]} = {arguments[0]}";

        return $"{mockName}.{method.Name}{TypeArguments(method)}({string.Join(", ", arguments)})";
    }

    private static bool IsDelegateInvoke(MethodInfo method)
        => method.Name == nameof(Action.Invoke) && typeof(Delegate).IsAssignableFrom(method.DeclaringType);

    private static string TypeArguments(MethodInfo method)
        => method.IsGenericMethod
            ? $"<{string.Join(", ", method.GetGenericArguments().Select(type => type.Alias()))}>"
            : string.Empty;
}
