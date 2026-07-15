namespace TSpec.Internal.TestData;

internal class Mutator()
{
    private readonly Dictionary<Type, Func<object, object>> _defaultSetups = [];

    internal void AddMutation(Type type, Func<object, object> setup)
        => _defaultSetups[type] =
        _defaultSetups.TryGetValue(type, out var previousSetup)
        ? MergeDefaultSetups(previousSetup, setup)
        : setup;

    internal object? Mutate(Type type, object? newValue)
        => newValue is not null && _defaultSetups.TryGetValue(type, out var setup)
            ? setup(newValue)
            : newValue;

    internal bool HasMutation(Type type) => _defaultSetups.ContainsKey(type);

    private static Func<object, object> MergeDefaultSetups(Func<object, object> setup1, Func<object, object> setup2)
        => obj => setup2(setup1(obj));
}