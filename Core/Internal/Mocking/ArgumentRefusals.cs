using System.Linq.Expressions;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// The forms an argument of a setup or verification may not take, each refused saying what to write instead.
internal static class ArgumentRefusals
{
    internal static void Check(Expression argument, ParameterInfo parameter, string callName)
    {
        var call = ArgumentMatcher.UnwrapConversion(argument) as MethodCallExpression;
        if (call is not null && ArgumentMatcher.IsAny(call.Method) && !argument.Type.IsAssignableFrom(call.Type))
            throw ConvertedAnyNeverMatches(call, argument.Type);
        if (call is not null && ArgumentMatcher.IsAnyMatcher(call.Method))
            return;

        if (call?.Method.DeclaringType?.FullName == "Moq.It")
            throw MoqsIt(call);
        if (NestedAny.TryFind(argument, out var nestedAny))
            throw NestedAnyMatchesNothing(nestedAny, parameter, callName);
        if (MockRead.TryFind(argument, out var mockMember))
            throw ReadsTheMock(parameter, callName, mockMember);
    }

    private static SetupFailed ConvertedAnyNeverMatches(MethodCallExpression any, Type parameterType)
    {
        var arguments = any.Arguments.Count == 0 ? "" : "...";
        var anyType = any.Type.Alias();
        var received = parameterType.Alias();
        return new(
            $"Any<{anyType}>({arguments}) is converted to {received}, so it can never match: "
            + $"the call receives {received.WithArticle()}, not {anyType.WithArticle()}. "
            + $"Write Any<{received}>({arguments}) instead");
    }

    private static SetupFailed MoqsIt(MethodCallExpression it)
        => new($"It.{it.Method.Name}<{it.Type.Alias()}>({(it.Arguments.Count == 0 ? "" : "...")}) is Moq's, "
            + "which TSpec does not use. Write Any<T>() for any value, "
            + "or Any<T>(constraint) for any value satisfying the constraint");

    private static SetupFailed NestedAnyMatchesNothing(MethodCallExpression any, ParameterInfo parameter, string callName)
    {
        var arguments = any.Arguments.Count == 0 ? "" : "...";
        var parameterType = parameter.ParameterType.Alias();
        return new(
            $"Any<{any.Type.Alias()}>({arguments}) matches a whole argument, not a part of one, "
            + $"so inside the {parameter.Name} argument of {callName} it can match nothing. "
            + $"Write Any<{parameterType}>() for any {parameterType}, "
            + $"or Any<{parameterType}>({parameter.Name} => ...) for any {parameterType} satisfying a condition");
    }

    private static SetupFailed ReadsTheMock(ParameterInfo parameter, string callName, string mockMember)
        => new($"The {parameter.Name} argument of {callName} reads {mockMember} from the mock itself, "
            + "but arguments are read before any call is made. "
            + $"Set up {mockMember} to return a value, and write that value as the argument");
}
