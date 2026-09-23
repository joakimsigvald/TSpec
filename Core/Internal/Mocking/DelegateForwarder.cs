using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// A delegate cannot be proxied, so a mocked one is compiled: a lambda of the delegate's own
/// signature that hands its arguments to the mock as a call to the delegate's Invoke.
/// </summary>
internal static class DelegateForwarder
{
    internal static Delegate Create(Type delegateType, Func<MethodInfo, object?[], object?> receive)
    {
        var invoke = delegateType.GetMethod(nameof(Action.Invoke))!;
        var parameters = invoke.GetParameters()
            .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
            .ToArray();
        var arguments = Expression.Variable(typeof(object[]), "arguments");
        var answer = Expression.Variable(typeof(object), "answer");
        Expression[] steps =
        [
            CollectArguments(parameters, arguments),
            ReceiveCall(receive, invoke, arguments, answer),
            .. WriteBackOutAndRefArguments(parameters, arguments),
            ReturnAnswer(answer, invoke.ReturnType)
        ];
        return Expression.Lambda(delegateType, Expression.Block([arguments, answer], steps), parameters).Compile();
    }

    private static BinaryExpression CollectArguments(ParameterExpression[] parameters, ParameterExpression arguments)
        => Expression.Assign(
            arguments,
            Expression.NewArrayInit(typeof(object), parameters.Select(parameter => Expression.Convert(parameter, typeof(object)))));

    private static BinaryExpression ReceiveCall(
        Func<MethodInfo, object?[], object?> receive,
        MethodInfo invoke,
        ParameterExpression arguments,
        ParameterExpression answer)
        => Expression.Assign(answer, Expression.Invoke(Expression.Constant(receive), Expression.Constant(invoke), arguments));

    private static IEnumerable<Expression> WriteBackOutAndRefArguments(
        ParameterExpression[] parameters, ParameterExpression arguments)
        => parameters
            .Select((parameter, index) => (parameter, index))
            .Where(_ => _.parameter.IsByRef)
            .Select(_ => Expression.Assign(_.parameter, ArgumentAt(arguments, _.index, _.parameter.Type)));

    private static UnaryExpression ArgumentAt(ParameterExpression arguments, int index, Type type)
        => Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(index)), type);

    private static Expression ReturnAnswer(ParameterExpression answer, Type returnType)
        => returnType == typeof(void) ? Expression.Empty() : Expression.Convert(answer, returnType);
}
