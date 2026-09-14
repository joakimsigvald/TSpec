using System.Linq.Expressions;
using TSpec.Internal.Specification;

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

        if (!IsReachedFrom(receiver, service))
            return false;

        if (!MockingStrategy.IsMockable(receiver.Type))
            throw NotMockable(receiver, body);

        var child = Expression.Parameter(receiver.Type, "_");
        link = Expression.Lambda(receiver, service);
        last = Expression.Lambda(WithReceiver(body, child), child);
        return true;
    }

    private static SetupFailed NotMockable(Expression receiver, Expression call)
    {
        var returning = CallMatcher.Unwrap(receiver);
        return new SetupFailed(
            $"{ReceiverOf(returning)!.Type.Alias()}.{MemberName(returning)} returns {receiver.Type.Alias().WithArticle()}, "
            + $"which TSpec does not mock, so {MemberName(call)} cannot be set up or verified through it");
    }

    private static string MemberName(Expression call)
        => call switch
        {
            MethodCallExpression methodCall => methodCall.Method.Name,
            MemberExpression member => member.Member.Name,
            _ => nameof(Action.Invoke)
        };

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
