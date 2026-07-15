using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TSpec.Test")]
namespace TSpec.Internal.Specification;

internal static class TypeExtensions
{
    internal static string Alias(this Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var rank = type.GetArrayRank();
            return $"{elementType.Alias()}[{new string(',', rank - 1)}]";
        }
        if (type.IsGenericType)
        {
            var genericTypeNames = type.GenericTypeArguments.Select(t => t.Alias()).ToArray();
            return $"{type.GenericBaseName()}<{string.Join(", ", genericTypeNames)}>";
        }
        return _typeAliases.TryGetValue(type, out var alias) ? alias : type.Name;
    }

    private static string GenericBaseName(this Type type) => type.Name.Split('`')[0];

    private static readonly Dictionary<Type, string> _typeAliases = new()
    {
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(object), "object" },
        { typeof(bool), "bool" },
        { typeof(char), "char" },
        { typeof(string), "string" },
        { typeof(void), "void" },
        { typeof(byte?), "byte?" },
        { typeof(sbyte?), "sbyte?" },
        { typeof(short?), "short?" },
        { typeof(ushort?), "ushort?" },
        { typeof(int?), "int?" },
        { typeof(uint?), "uint?" },
        { typeof(long?), "long?" },
        { typeof(ulong?), "ulong?" },
        { typeof(float?), "float?" },
        { typeof(double?), "double?" },
        { typeof(decimal?), "decimal?" },
        { typeof(bool?), "bool?" },
        { typeof(char?), "char?" }
    };
}