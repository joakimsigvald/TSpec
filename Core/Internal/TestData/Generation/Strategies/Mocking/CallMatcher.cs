using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// Which calls a setup or a verification is about: one member of the mocked service, and what its
/// arguments must be. An argument written as a value matches an equal one — a collection, one with
/// the same elements; <c>Any&lt;T&gt;()</c> matches any T, and <c>Any&lt;T&gt;(constraint)</c> any T
/// satisfying the constraint. Values are read once, as the matcher is made.
/// </summary>
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

    internal Type ReturnType => _method.ReturnType;

    /// A member named because no expression can name it; a name states no arguments, so any match.
    internal static CallMatcher For(MemberInfo member)
        => new(member is PropertyInfo property ? property.GetMethod! : (MethodInfo)member, null, []);

    internal static CallMatcher For(LambdaExpression call)
    {
        var service = call.Parameters[0];
        var (method, arguments) = Unwrap(call.Body) switch
        {
            MethodCallExpression { Object: var target } methodCall when IsService(target, service)
                => (methodCall.Method, methodCall.Arguments),
            MemberExpression { Member: PropertyInfo { GetMethod: { } getter }, Expression: var target }
                when IsService(target, service)
                => (getter, (IReadOnlyList<Expression>)[]),
            InvocationExpression { Expression: var target } invocation when IsService(target, service)
                => (target!.Type.GetMethod(nameof(Action.Invoke))!, invocation.Arguments),
            _ => throw new SetupFailed(
                $"'{call}' does not call a member of the mocked {service.Type.Alias()}, so it names no call to match")
        };
        AssertInterceptable(method, service.Type);
        var parameters = method.GetParameters();
        var outValues = parameters
            .Select((parameter, index) => (parameter, index))
            .Where(_ => _.parameter.IsOut)
            .Select(_ => (_.index, Evaluate(arguments[_.index])))
            .ToArray();
        var matchers = arguments
            .Select((argument, index) => parameters[index].IsOut ? (_ => true) : ArgumentMatcher(argument))
            .ToArray();
        return new(method, matchers, outValues);
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

    private static bool IsAny(MethodInfo method)
        => method is { Name: nameof(Spec.Any), IsGenericMethod: true }
        && method.DeclaringType is { IsGenericType: true } declaringType
        && declaringType.GetGenericTypeDefinition() == typeof(Spec<,>);

    private static bool IsConstraint(MethodInfo method)
        => method.GetParameters() is [{ ParameterType: var type }]
        && type == typeof(Func<,>).MakeGenericType(method.ReturnType, typeof(bool));

    private static bool IsService(Expression? target, ParameterExpression service)
        => target is not null && Unwrap(target) == service;

    private static Expression Unwrap(Expression expression)
        => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert
            ? Unwrap(convert.Operand)
            : expression;

    private static void AssertInterceptable(MethodInfo method, Type service)
    {
        if (method.DeclaringType is { IsInterface: true } || typeof(Delegate).IsAssignableFrom(method.DeclaringType))
            return;
        if (method.IsVirtual && !method.IsFinal)
            return;
        throw new SetupFailed(
            $"{service.Alias()}.{method.Name} is not virtual or abstract, so nothing can intercept it. "
            + "Only a member the mock can override may be set up or verified");
    }

    private static Func<object?, bool> ArgumentMatcher(Expression argument)
    {
        if (Unwrap(argument) is MethodCallExpression call)
        {
            if (IsAny(call.Method) && call.Arguments.Count == 0)
                return AnyOf(call.Type);
            if (IsAny(call.Method) && IsConstraint(call.Method))
                return Satisfying(call.Type, call.Arguments[0]);
            if (call.Method.DeclaringType?.FullName == "Moq.It")
                throw new SetupFailed(
                    $"It.{call.Method.Name}<{call.Type.Alias()}>({(call.Arguments.Count == 0 ? "" : "...")}) is Moq's, "
                    + "which TSpec does not use. Write Any<T>() for any value, "
                    + "or Any<T>(constraint) for any value satisfying the constraint");
        }
        var expected = Evaluate(argument);
        return actual => AreEqual(expected, actual);
    }

    private static Func<object?, bool> AnyOf(Type type)
        => actual => actual is null ? !type.IsValueType || Nullable.GetUnderlyingType(type) is not null : type.IsInstanceOfType(actual);

    private static Func<object?, bool> Satisfying(Type type, Expression constraint)
    {
        var value = Expression.Parameter(typeof(object), "value");
        var test = Expression.Lambda<Func<object?, bool>>(
            Expression.Invoke(constraint, Expression.Convert(value, type)), value).Compile();
        var isOfType = AnyOf(type);
        return actual => isOfType(actual) && test(actual);
    }

    private static object? Evaluate(Expression expression)
        => expression is ConstantExpression constant
            ? constant.Value
            : Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object)))
                .Compile(preferInterpretation: true)();

    private static bool AreEqual(object? expected, object? actual)
        => Equals(expected, actual)
        || expected is IEnumerable expectedItems and not string
            && actual is IEnumerable actualItems and not string
            && expectedItems.Cast<object?>().SequenceEqual(actualItems.Cast<object?>());

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
