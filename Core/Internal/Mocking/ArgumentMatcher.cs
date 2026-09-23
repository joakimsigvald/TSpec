using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace TSpec.Internal.Mocking;

/// <summary>
/// What an argument of a setup or verification matches. An argument written as a value matches an
/// equal one — a collection, one with the same elements; <c>Any&lt;T&gt;()</c> matches any T, and
/// <c>Any&lt;T&gt;(constraint)</c> any T satisfying the constraint. Values are read once, as the
/// matcher is made.
/// </summary>
internal static class ArgumentMatcher
{
    internal static Func<object?, bool> For(Expression argument, ParameterInfo parameter, string callName)
    {
        ArgumentRefusals.Check(argument, parameter, callName);
        if (UnwrapConversion(argument) is MethodCallExpression call)
        {
            if (IsAny(call.Method) && call.Arguments.Count == 0)
                return AnyOf(call.Type);
            if (IsAny(call.Method) && IsConstraint(call.Method))
                return Satisfying(call.Type, call.Arguments[0]);
        }

        return EqualTo(Evaluate(argument));
    }

    internal static Func<object?, bool> EqualTo(object? expected) => actual => AreEqual(expected, actual);

    internal static object? Evaluate(Expression expression)
        => expression is ConstantExpression constant
            ? constant.Value
            : Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object)))
                .Compile(preferInterpretation: true)();

    internal static bool IsAny(MethodInfo method)
        => method is { Name: nameof(Spec.Any), IsGenericMethod: true }
        && method.DeclaringType is { IsGenericType: true } declaringType
        && declaringType.GetGenericTypeDefinition() == typeof(Spec<,>);

    /// The forms of Any that mean a match; Any with a setup yields a value wherever it is written.
    internal static bool IsAnyMatcher(MethodInfo method)
        => IsAny(method) && (method.GetParameters().Length == 0 || IsConstraint(method));

    internal static Expression UnwrapConversion(Expression expression)
        => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert
            ? UnwrapConversion(convert.Operand)
            : expression;

    private static bool IsConstraint(MethodInfo method)
        => method.GetParameters() is [{ ParameterType: var type }]
        && type == typeof(Func<,>).MakeGenericType(method.ReturnType, typeof(bool));

    private static Func<object?, bool> AnyOf(Type type)
        => actual => actual is null ? !type.IsValueType || Nullable.GetUnderlyingType(type) is not null : type.IsInstanceOfType(actual);

    private static Func<object?, bool> Satisfying(Type type, Expression constraint)
    {
        var value = Expression.Parameter(typeof(object), "value");
        var test = Expression.Lambda<Func<object?, bool>>(
            Expression.Invoke(constraint, Expression.Convert(value, type)), value).Compile();
        var isOfType = AnyOf(type);
        return actual => isOfType(actual) && test(actual);
    }

    private static bool AreEqual(object? expected, object? actual)
        => Equals(expected, actual)
        || expected is IEnumerable expectedItems and not string
            && actual is IEnumerable actualItems and not string
            && expectedItems.Cast<object?>().SequenceEqual(actualItems.Cast<object?>());
}
