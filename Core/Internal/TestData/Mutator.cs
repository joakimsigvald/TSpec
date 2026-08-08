namespace TSpec.Internal.TestData;

/// <summary>
/// The setups registered per type, each with the scope it was registered for. They are kept as a
/// list rather than merged on arrival, since which of them apply is not known until a value is
/// asked for: a request carries the scope it is asked in, and only the setups whose scope
/// intersects it run. Order of application is order of registration, as when they were merged.
/// </summary>
internal class Mutator()
{
    private readonly Dictionary<Type, List<(For Scope, Func<object, object> Setup)>> _defaultSetups = [];

    internal void AddMutation(Type type, For scope, Func<object, object> setup)
    {
        if (!_defaultSetups.TryGetValue(type, out var setups))
            _defaultSetups[type] = setups = [];
        setups.Add((scope, setup));
    }

    internal object? Mutate(Type type, object? newValue, For scope)
    {
        if (newValue is null || !_defaultSetups.TryGetValue(type, out var setups))
            return newValue;

        foreach (var (registered, setup) in setups)
            if (Applies(registered, scope))
                newValue = setup(newValue);
        return newValue;
    }

    /// <summary>
    /// Whether a setup alone is reason enough to produce a default value of the type. A setup out
    /// of scope is not: answering yes there would hand the caller a value built by a setup that
    /// does not apply to it, which is the scope leaking rather than holding.
    /// </summary>
    internal bool HasMutation(Type type, For scope)
        => _defaultSetups.TryGetValue(type, out var setups)
        && setups.Any(entry => Applies(entry.Scope, scope));

    private static bool Applies(For registered, For requested) => (registered & requested) != For.None;
}
