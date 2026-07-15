namespace TSpec.Internal.TestData;

using TSpec.Internal.TestData.Generation.Strategies.IlCompilation;
using Arrangement = (bool HasValue, object? Value, Func<object?>? Factory);

internal class DataProvider
{
    private readonly Dictionary<Type, Arrangement> _generalDefaults = [];
    private readonly Dictionary<Type, Arrangement> _inputDefaults = [];
    private readonly Dictionary<Type, Arrangement> _subjectDefaults = [];

    internal void UseValue<TValue>(TValue value, For scope)
    {
        var type = typeof(TValue);
        var defaults = GetDefaults(scope);
        defaults[type] = new Arrangement(true, value, null);
        foreach (var iface in type.GetInterfaces())
            defaults[iface] = new Arrangement(true, value, null);
    }

    internal void UseFactory<TValue>(Func<TValue> factory, For scope)
    {
        var type = typeof(TValue);
        var defaults = GetDefaults(scope);
        var sharedInstance = new Lazy<TValue>(factory);
        defaults[type] = ArrangeFactory(defaults, type, () => sharedInstance.Value);
        foreach (var iface in type.GetInterfaces())
            defaults[iface] = ArrangeFactory(defaults, iface, () => sharedInstance.Value);
    }

    private static Arrangement ArrangeFactory<TValue>(Dictionary<Type, Arrangement> defaults, Type key, Func<TValue> factory)
    {
        if (!defaults.TryGetValue(key, out var current))
            return new(false, null, () => factory());

        if (current.Factory is null)
            return new(current.HasValue, current.Value, () => factory());

        var oldFactory = current.Factory;
        return new(current.HasValue, current.Value, () => { oldFactory(); return factory(); });
    }

    private Dictionary<Type, Arrangement> GetDefaults(For scope)
        => scope switch
        {
            For.Input => _inputDefaults,
            For.Subject => _subjectDefaults,
            For.All => _generalDefaults,
            _ => throw new NotImplementedException($"{scope}")
        };

    public bool TryGetValue(Type type, For scope, out object? val)
        => TryGetValueOfType(type, scope, out val) || TryGetValueOfTask(type, scope, out val);

    private bool TryGetValueOfType(Type type, For scope, out object? val)
        => scope switch
        {
            For.Input => TryGetValue(_inputDefaults, type, out val) || TryGetValue(_generalDefaults, type, out val),
            For.Subject => TryGetValue(_subjectDefaults, type, out val) || TryGetValue(_generalDefaults, type, out val),
            _ => throw new NotImplementedException($"{scope}")
        };

    private bool TryGetValueOfTask(Type type, For scope, out object? val)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = type.GetGenericArguments()[0];
            if (TryGetValue(innerType, scope, out var innerVal))
            {
                val = TaskCompiler.GetFromResultMethod(innerType)(innerVal!);
                return true;
            }
        }
        val = null;
        return false;
    }

    private static bool TryGetValue(Dictionary<Type, Arrangement> arrangements, Type type, out object? val)
    {
        if (arrangements.TryGetValue(type, out var arr) && (arr.HasValue || arr.Factory != null))
        {
            if (arr.Factory != null)
            {
                arrangements[type] = new(arr.HasValue, arr.Value, null);
                arrangements[type] = arr = new(true, arr.Factory(), null);
            }
            val = arr.Value;
            return true;
        }
        val = null;
        return false;
    }
}