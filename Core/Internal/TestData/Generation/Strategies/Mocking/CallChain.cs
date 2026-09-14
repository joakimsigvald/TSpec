using System.Linq.Expressions;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// A call reached through members of the mocked service, such as <c>_ => _.Child.Get(1)</c>: the
/// last call is made on the mock of its receiver's type, which the rest of the chain answers with.
/// </summary>
internal static class CallChain
{
    internal static bool TrySplit(LambdaExpression call, out LambdaExpression link, out LambdaExpression last)
    {
        link = last = null!;
        var service = call.Parameters[0];
        var body = CallMatcher.Unwrap(call.Body);
        var receiver = ReceiverOf(body);
        if (receiver is null || CallMatcher.Unwrap(receiver) == service)
            return false;

        if (!IsReachedFrom(receiver, service) || !MockingStrategy.IsMocked(receiver.Type))
            return false;

        var child = Expression.Parameter(receiver.Type, "_");
        link = Expression.Lambda(receiver, service);
        last = Expression.Lambda(WithReceiver(body, child), child);
        return true;
    }

    private static Expression? ReceiverOf(Expression call)
        => call switch
        {
            MethodCallExpression methodCall => methodCall.Object,
            MemberExpression member => member.Expression,
            InvocationExpression invocation => invocation.Expression,
            _ => null
        };

    private static bool IsReachedFrom(Expression? expression, ParameterExpression service)
        => expression is not null
        && (CallMatcher.Unwrap(expression) == service
            || IsReachedFrom(ReceiverOf(CallMatcher.Unwrap(expression)), service));

    private static Expression WithReceiver(Expression call, ParameterExpression receiver)
        => call switch
        {
            MethodCallExpression methodCall => methodCall.Update(receiver, methodCall.Arguments),
            MemberExpression member => member.Update(receiver),
            InvocationExpression invocation => invocation.Update(receiver, invocation.Arguments),
            _ => call
        };
}
