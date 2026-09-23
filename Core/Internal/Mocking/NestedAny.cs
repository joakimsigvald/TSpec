using System.Linq.Expressions;

namespace TSpec.Internal.Mocking;

/// An argument that is not itself Any, but has one inside it.
internal sealed class NestedAny : ExpressionVisitor
{
    private MethodCallExpression? _found;

    internal static bool TryFind(Expression argument, out MethodCallExpression any)
    {
        var visitor = new NestedAny();
        visitor.Visit(argument);
        any = visitor._found!;
        return visitor._found is not null;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!ArgumentMatcher.IsAnyMatcher(node.Method))
            return base.VisitMethodCall(node);

        _found ??= node;
        return node;
    }
}
