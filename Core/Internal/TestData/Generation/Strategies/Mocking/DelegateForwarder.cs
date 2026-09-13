using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

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
        var arguments = Expression.NewArrayInit(
            typeof(object), parameters.Select(parameter => Expression.Convert(parameter, typeof(object))));
        var call = Expression.Invoke(Expression.Constant(receive), Expression.Constant(invoke), arguments);
        Expression body = invoke.ReturnType == typeof(void) ? call : Expression.Convert(call, invoke.ReturnType);
        return Expression.Lambda(delegateType, body, parameters).Compile();
    }
}
