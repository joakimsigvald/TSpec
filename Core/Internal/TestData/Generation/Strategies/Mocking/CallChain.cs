using System.Linq.Expressions;
using TSpec.Internal.Specification;

namespace TSpec.Internal.TestData.Generation.Strategies.Mocking;

/// <summary>
/// A call reached through members of the mocked service, such as <c>_ => _.GetChild(2).Get(1)</c>: its
/// first step, <c>_.GetChild(2)</c>, is a call on the service answering with a child, and the rest,
/// <c>_ => _.Get(1)</c>, is a call on that child.
/// </summary>
internal static class CallChain
{
    internal static bool TrySplit(LambdaExpression call, out LambdaExpression firstStep, out LambdaExpression rest)
    {
        firstStep = rest = null!;
        var service = call.Parameters[0];
        var body = CallMatcher.Unwrap(call.Body);
        var receiver = ReceiverOf(body);
        if (receiver is null || CallMatcher.Unwrap(receiver) == service)
            return false;

        if (!IsReachedFrom(receiver, service))
            return false;

        var (step, calledOnStep) = FirstStepOf(body, service);
        if (!MockingStrategy.IsMockedByDefault(step.Type))
            throw NotMockable(step, calledOnStep);

        var child = Expression.Parameter(step.Type, "_");
        firstStep = Expression.Lambda(step, service);
        rest = Expression.Lambda(Replace(body, step, child), child);
        return true;
    }

    private static (Expression Step, Expression CalledOnStep) FirstStepOf(Expression body, ParameterExpression service)
    {
        var calledOnStep = body;
        var step = ReceiverOf(body)!;
        while (CallMatcher.Unwrap(ReceiverOf(CallMatcher.Unwrap(step))!) != service)
        {
            calledOnStep = CallMatcher.Unwrap(step);
            step = ReceiverOf(calledOnStep)!;
        }
        return (step, calledOnStep);
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

    private static Expression Replace(Expression node, Expression step, ParameterExpression child)
    {
        if (node == step)
            return child;

        return node is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert
            ? convert.Update(Replace(convert.Operand, step, child))
            : WithReceiver(node, Replace(ReceiverOf(node)!, step, child));
    }

    private static Expression WithReceiver(Expression call, Expression receiver)
        => call switch
        {
            MethodCallExpression methodCall => methodCall.Update(receiver, methodCall.Arguments),
            MemberExpression member => member.Update(receiver),
            InvocationExpression invocation => invocation.Update(receiver, invocation.Arguments),
            _ => call
        };
}
