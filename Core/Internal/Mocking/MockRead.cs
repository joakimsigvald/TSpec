using System.Linq.Expressions;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Mocking;

/// <summary>
/// The first member an argument reads on the mock: on the parameter of the call's lambda, or of the
/// chain it was split from, rather than on one a lambda inside the argument declares.
/// </summary>
internal sealed class MockRead : ExpressionVisitor
{
    private readonly HashSet<ParameterExpression> _declared = [];
    private string? _found;

    internal static bool TryFind(Expression argument, out string mockMember)
    {
        var visitor = new MockRead();
        visitor.Visit(argument);
        mockMember = visitor._found!;
        return visitor._found is not null;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        _declared.UnionWith(node.Parameters);
        return base.VisitLambda(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (IsMock(node.Expression))
            _found ??= $"{node.Expression!.Type.Alias()}.{node.Member.Name}";
        return base.VisitMember(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (IsMock(node.Object))
            _found ??= $"{node.Object!.Type.Alias()}.{node.Method.Name}";
        return base.VisitMethodCall(node);
    }

    private bool IsMock(Expression? receiver)
        => receiver is not null
        && CallReader.Unwrap(receiver) is ParameterExpression parameter
        && !_declared.Contains(parameter);
}
