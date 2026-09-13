using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// In a mock setup or verification, <c>Any&lt;T&gt;()</c> means any T and <c>Any&lt;T&gt;(constraint)</c>
/// any T satisfying the constraint: the expression handed to Moq has each replaced by
/// <c>It.IsAny&lt;T&gt;()</c> and <c>It.Is&lt;T&gt;(constraint)</c>.
/// </summary>
internal sealed class AnyArgument : ExpressionVisitor
{
    private static readonly AnyArgument _instance = new();

    internal static Expression<TDelegate> Rewrite<TDelegate>(Expression<TDelegate> expression)
        => (Expression<TDelegate>)_instance.Visit(expression);

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!IsAny(node.Method))
            return base.VisitMethodCall(node);

        if (node.Arguments.Count == 0)
            return Expression.Call(typeof(It), nameof(It.IsAny), [node.Type]);

        return IsConstraint(node.Method)
            ? Expression.Call(typeof(It), nameof(It.Is), [node.Type], Expression.Quote(AsLambda(node.Arguments[0], node.Type)))
            : base.VisitMethodCall(node);
    }

    private static bool IsAny(MethodInfo method)
        => method is { Name: nameof(Spec.Any), IsGenericMethod: true }
        && method.DeclaringType is { IsGenericType: true } declaringType
        && declaringType.GetGenericTypeDefinition() == typeof(Spec<,>);

    private static bool IsConstraint(MethodInfo method)
        => method.GetParameters() is [{ ParameterType: var type }]
        && type == typeof(Func<,>).MakeGenericType(method.ReturnType, typeof(bool));

    /// A lambda written in place is the constraint itself; any other delegate is invoked by one.
    private static LambdaExpression AsLambda(Expression constraint, Type valueType)
    {
        if (constraint is LambdaExpression lambda)
            return lambda;

        var value = Expression.Parameter(valueType, "value");
        return Expression.Lambda(Expression.Invoke(constraint, value), value);
    }
}
