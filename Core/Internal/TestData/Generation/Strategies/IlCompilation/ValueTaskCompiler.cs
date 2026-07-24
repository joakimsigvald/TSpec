using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class ValueTaskCompiler
{
    private static readonly ParameterExpression _value = Parameter(typeof(object), "value");
    private static readonly ConcurrentDictionary<Type, Func<object, object>> _cache = [];

    internal static Func<object, object> GetFromResultMethod(Type valueType)
        => _cache.GetOrAdd(valueType, CompileValueTaskFromResult);

    private static Func<object, object> CompileValueTaskFromResult(Type valueType)
        => Lambda<Func<object, object>>(BuildConstruction(valueType), _value).Compile();

    private static UnaryExpression BuildConstruction(Type valueType)
        => Convert(New(GetResultConstructor(valueType), CastValue(valueType)), typeof(object));

    private static ConstructorInfo GetResultConstructor(Type valueType)
        => typeof(ValueTask<>).MakeGenericType(valueType).GetConstructor([valueType])!;

    private static UnaryExpression CastValue(Type valueType)
        => Convert(_value, valueType);
}
