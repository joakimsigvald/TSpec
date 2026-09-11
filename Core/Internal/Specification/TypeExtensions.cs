using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TSpec.Test")]
namespace TSpec.Internal.Specification;

internal static class TypeExtensions
{
    /// <summary>
    /// The type as C# writes it. <paramref name="names"/> are the tuple element names the compiler
    /// recorded where the type was written, taken in the order it lists them: a tuple's own names,
    /// then those of the tuples inside it.
    /// </summary>
    internal static string Alias(this Type type, Queue<string?>? names = null)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var rank = type.GetArrayRank();
            return $"{elementType.Alias(names)}[{new string(',', rank - 1)}]";
        }
        if (type.IsGenericType && _tuples.Contains(type.GetGenericTypeDefinition()))
            return Tuple(type, names);
        if (type.IsGenericType)
        {
            var genericTypeNames = type.GenericTypeArguments.Select(t => t.Alias(names)).ToArray();
            return $"{type.GenericBaseName()}<{string.Join(", ", genericTypeNames)}>";
        }
        return _typeAliases.TryGetValue(type, out var alias) ? alias : type.Name;
    }

    /// <summary>
    /// The compiler nests the eighth element on in a tuple of its own, and lists names for that one
    /// too — always none — so they are taken and dropped to keep the rest in step.
    /// </summary>
    private static string Tuple(Type tuple, Queue<string?>? names)
    {
        var own = Take(names, Arity(tuple));
        var elements = new List<string>();
        for (var part = tuple; ; )
        {
            var args = part.GenericTypeArguments;
            elements.AddRange(args.Take(7).Select(arg => arg.Alias(names)));
            if (args.Length < 8)
                break;
            part = args[7];
            Take(names, Arity(part));
        }
        return $"({string.Join(", ", elements.Select((element, i) => own[i] is { } name ? $"{element} {name}" : element))})";
    }

    private static int Arity(Type tuple)
        => tuple.GenericTypeArguments is { Length: 8 } args ? 7 + Arity(args[7]) : tuple.GenericTypeArguments.Length;

    private static string?[] Take(Queue<string?>? names, int count)
        => [.. Enumerable.Range(0, count).Select(_ => names is { Count: > 0 } ? names.Dequeue() : null)];

    private static readonly HashSet<Type> _tuples =
    [
        typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>),
    ];

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