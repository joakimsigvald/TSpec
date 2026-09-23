using System.Linq.Expressions;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// <summary>
/// The member a call lambda calls on the mocked service, and the argument expressions it passes. A set,
/// which no expression can write as an assignment, is named with <c>Set(property, value)</c>; a read to
/// verify, which is no statement, with <c>Get(property)</c>.
/// </summary>
internal static class CallReader
{
    internal static (MethodInfo Method, IReadOnlyList<Expression> Arguments) Read(LambdaExpression call)
    {
        var service = call.Parameters[0];
        var (method, arguments) = Unwrap(call.Body) switch
        {
            MethodCallExpression { Object: null } set when IsMarker(set.Method, nameof(Spec.Set))
                => SetterCall(set, service) ?? throw NamesNoCall(call, service),
            MethodCallExpression { Object: null } get when IsMarker(get.Method, nameof(Spec.Get))
                => GetterCall(get, service) ?? throw NamesNoCall(call, service),
            MethodCallExpression { Object: var target } methodCall when IsService(target, service)
                => (methodCall.Method, methodCall.Arguments),
            MemberExpression { Member: PropertyInfo { GetMethod: { } getter }, Expression: var target }
                when IsService(target, service)
                => (getter, (IReadOnlyList<Expression>)[]),
            InvocationExpression { Expression: var target } invocation when IsService(target, service)
                => (target!.Type.GetMethod(nameof(Action.Invoke))!, invocation.Arguments),
            _ => throw NamesNoCall(call, service)
        };
        AssertInterceptable(method, service.Type);
        return (method, arguments);
    }

    /// A read is set up as the property itself, which a setup can answer; Get names one only to verify.
    internal static void AssertIsNotARead(LambdaExpression call)
    {
        var service = call.Parameters[0];
        if (Unwrap(call.Body) is MethodCallExpression { Object: null } get
            && IsMarker(get.Method, nameof(Spec.Get))
            && PropertyAccess(get.Arguments[0], service) is { } access)
            throw ReadInASetup(service.Type, access);
    }

    /// A call reads the same converted, or awaited by its task's Result, which no expression can await.
    internal static Expression Unwrap(Expression expression)
        => expression switch
        {
            UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert
                => Unwrap(convert.Operand),
            MemberExpression { Member.Name: "Result", Expression: { } task } when AsyncAnswer.IsAsyncOfValue(task.Type)
                => Unwrap(task),
            _ => expression
        };

    private static bool IsService(Expression? target, ParameterExpression service)
        => target is not null && Unwrap(target) == service;

    private static bool IsMarker(MethodInfo method, string name)
        => method.Name == name
        && method.IsGenericMethod
        && method.DeclaringType is { IsGenericType: true } declaringType
        && declaringType.GetGenericTypeDefinition() == typeof(Spec<,>);

    /// A property's setter takes what its getter does, the indexes of an indexer, and then the value.
    private static (MethodInfo, IReadOnlyList<Expression>)? SetterCall(MethodCallExpression set, ParameterExpression service)
        => PropertyAccess(set.Arguments[0], service) is { } access
            ? (SetterOf(access.Property, service.Type), [.. access.Indexes, set.Arguments[1]])
            : null;

    private static (MethodInfo, IReadOnlyList<Expression>)? GetterCall(MethodCallExpression get, ParameterExpression service)
        => PropertyAccess(get.Arguments[0], service) is { } access
            ? (access.Property.GetMethod!, access.Indexes)
            : null;

    private static (PropertyInfo Property, IReadOnlyList<Expression> Indexes)? PropertyAccess(
        Expression expression, ParameterExpression service)
        => Unwrap(expression) switch
        {
            MemberExpression { Member: PropertyInfo property, Expression: var target } when IsService(target, service)
                => (property, []),
            MethodCallExpression { Object: var target, Method: var getter } indexer
                when IsService(target, service) && PropertyOf(getter) is { } property
                => (property, indexer.Arguments),
            _ => null
        };

    private static PropertyInfo? PropertyOf(MethodInfo getter)
        => getter.DeclaringType!.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(property => property.GetMethod == getter);

    private static MethodInfo SetterOf(PropertyInfo property, Type service)
        => property.SetMethod
        ?? throw new SetupFailed($"{service.Alias()}.{property.Name} has no setter, so it cannot be set");

    private static SetupFailed ReadInASetup(Type service, (PropertyInfo Property, IReadOnlyList<Expression> Indexes) access)
    {
        var shown = access.Indexes.Count == 0 ? $".{access.Property.Name}" : "[…]";
        return new($"Get names a read only in Then<T>(…); set up {service.Alias()}{shown} with That(_ => _{shown})");
    }

    private static SetupFailed NamesNoCall(LambdaExpression call, ParameterExpression service)
        => new($"'{call}' does not call a member of the mocked {service.Type.Alias()}, so it names no call to match");

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
}
