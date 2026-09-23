using System.Linq.Expressions;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// Which calls a setup or a verification is about: one member of the mocked service, and what its arguments must be.
internal sealed class CallMatcher
{
    private readonly MethodInfo _method;
    private readonly Func<object?, bool>[]? _arguments;
    private readonly (int Index, object? Value)[] _outValues;

    private CallMatcher(MethodInfo method, Func<object?, bool>[]? arguments, (int, object?)[] outValues)
    {
        _method = method;
        _arguments = arguments;
        _outValues = outValues;
    }

    internal MethodInfo Method => _method;

    internal Type ReturnType => _method.ReturnType;

    /// A member named because no expression can name it; a name states no arguments, so any match.
    internal static CallMatcher For(MemberInfo member)
        => new(member is PropertyInfo property ? property.GetMethod! : (MethodInfo)member, null, []);

    internal static CallMatcher Exactly(MethodInfo method, IReadOnlyList<object?> arguments)
        => new(method, [.. arguments.Select(ArgumentMatcher.EqualTo)], []);

    internal static CallMatcher For(LambdaExpression call)
    {
        var (method, arguments) = CallReader.Read(call);
        var parameters = method.GetParameters();
        var callName = $"{call.Parameters[0].Type.Alias()}.{method.Name}";
        var matchers = arguments
            .Select((argument, index) => parameters[index].IsOut
                ? (_ => true)
                : ArgumentMatcher.For(argument, parameters[index], callName))
            .ToArray();
        return new(method, matchers, OutValues(parameters, arguments));
    }

    internal bool Matches(MockInvocation invocation) => Matches(invocation.Method, invocation.Arguments);

    internal bool Matches(MethodInfo method, IReadOnlyList<object?> arguments)
        => IsSameMethod(_method, method)
        && (_arguments is null || _arguments.Select((matches, index) => matches(arguments[index])).All(matched => matched));

    /// A matching call hands back the out arguments the setup was written with.
    internal void WriteOutArguments(object?[] arguments)
    {
        foreach (var (index, value) in _outValues)
            arguments[index] = value;
    }

    private static (int, object?)[] OutValues(ParameterInfo[] parameters, IReadOnlyList<Expression> arguments)
        => [.. parameters
            .Select((parameter, index) => (parameter, index))
            .Where(_ => _.parameter.IsOut)
            .Select(_ => (_.index, ArgumentMatcher.Evaluate(arguments[_.index])))];

    /// <summary>
    /// The same member however it was reached: through the type that declares it or one that
    /// overrides it, and for a generic method, with the same type arguments.
    /// </summary>
    private static bool IsSameMethod(MethodInfo expected, MethodInfo actual)
    {
        if (expected == actual)
            return true;
        var expectedBase = expected.GetBaseDefinition();
        var actualBase = actual.GetBaseDefinition();
        return expectedBase.DeclaringType == actualBase.DeclaringType
            && expectedBase.HasSameMetadataDefinitionAs(actualBase)
            && expected.GetGenericArguments().SequenceEqual(actual.GetGenericArguments());
    }
}
