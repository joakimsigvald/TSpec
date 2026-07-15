using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using static System.Linq.Expressions.Expression;

namespace TSpec.Internal.TestData.Generation.Strategies.IlCompilation;

internal static class PropertyCompiler
{
    private readonly static ParameterExpression _instance = Parameter(typeof(object), "instance");
    private readonly static ParameterExpression _value = Parameter(typeof(object), "value");
    private static readonly ConcurrentDictionary<Type, CompiledProperty[]> _cache = [];

    internal static CompiledProperty[] GetAccessors(Type type) => _cache.GetOrAdd(type, CompileAllProperties);

    private static CompiledProperty[] CompileAllProperties(Type type)
        => [.. GetWritableProperties(type).Select(prop => CompileProperty(type, prop))];

    private static IEnumerable<PropertyInfo> GetWritableProperties(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite
                     && p.GetSetMethod(nonPublic: false) != null
                     && p.GetIndexParameters().Length == 0);

    private static CompiledProperty CompileProperty(Type declaringType, PropertyInfo property)
        => new(property.PropertyType, CompileGetter(declaringType, property), CompileSetter(declaringType, property));

    private static Func<object, object?> CompileGetter(Type declaringType, PropertyInfo property)
        => Lambda<Func<object, object?>>(BuildGetterBody(declaringType, property), _instance).Compile();

    private static Action<object, object?> CompileSetter(Type declaringType, PropertyInfo property)
        => Lambda<Action<object, object?>>(BuildSetterBody(declaringType, property), _instance, _value).Compile();

    private static UnaryExpression BuildGetterBody(Type declaringType, PropertyInfo property)
        => Convert(Property(ResolveTypedInstance(declaringType), property), typeof(object));

    private static BinaryExpression BuildSetterBody(Type declaringType, PropertyInfo property)
        => Assign(Property(ResolveTypedInstance(declaringType), property), Convert(_value, property.PropertyType));

    private static UnaryExpression ResolveTypedInstance(Type declaringType)
        => declaringType.IsValueType ? Unbox(_instance, declaringType) : Convert(_instance, declaringType);
}