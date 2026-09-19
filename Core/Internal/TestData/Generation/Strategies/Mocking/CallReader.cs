using System.Linq.Expressions;
using System.Reflection;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// The member a call lambda calls on the mocked service, and the argument expressions it passes.
internal static class CallReader
{
    internal static (MethodInfo Method, IReadOnlyList<Expression> Arguments) Read(LambdaExpression call)
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
        return (method, arguments);
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
