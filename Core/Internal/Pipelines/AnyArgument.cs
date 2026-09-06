using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// In a mock setup or verification, <c>Any&lt;T&gt;()</c> means any T: the expression handed to Moq
/// has each parameterless <c>Any&lt;T&gt;()</c> replaced by <c>It.IsAny&lt;T&gt;()</c>.
/// </summary>
internal sealed class AnyArgument : ExpressionVisitor
{
    private static readonly AnyArgument _instance = new();

    internal static Expression<TDelegate> Rewrite<TDelegate>(Expression<TDelegate> expression)
        => (Expression<TDelegate>)_instance.Visit(expression);

    protected override Expression VisitMethodCall(MethodCallExpression node)
        => IsAny(node.Method)
        ? Expression.Call(typeof(It), nameof(It.IsAny), [node.Type])
        : base.VisitMethodCall(node);

    private static bool IsAny(MethodInfo method)
        => method is { Name: nameof(Spec.Any), IsGenericMethod: true }
        && method.GetParameters().Length == 0
        && method.DeclaringType is { IsGenericType: true } declaringType
        && declaringType.GetGenericTypeDefinition() == typeof(Spec<,>);
}
