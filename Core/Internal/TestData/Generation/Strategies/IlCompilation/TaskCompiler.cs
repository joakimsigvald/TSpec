using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class TaskCompiler
{
    private static readonly ParameterExpression _value = Parameter(typeof(object), "value");
    private static readonly ConcurrentDictionary<Type, Func<object, Task>> _cache = [];

    internal static Func<object, Task> GetFromResultMethod(Type valueType)
        => _cache.GetOrAdd(valueType, CompileTaskFromResult);

    private static Func<object, Task> CompileTaskFromResult(Type valueType)
        => Lambda<Func<object, Task>>(BuildMethodCall(valueType), _value).Compile();

    private static UnaryExpression BuildMethodCall(Type valueType)
        => Convert(Call(GetGenericFromResultMethod(valueType), CastValue(valueType)), typeof(Task));

    private static MethodInfo GetGenericFromResultMethod(Type valueType)
        => typeof(Task).GetMethod("FromResult", BindingFlags.Public | BindingFlags.Static)!.MakeGenericMethod(valueType);

    private static UnaryExpression CastValue(Type valueType)
        => Convert(_value, valueType);
}