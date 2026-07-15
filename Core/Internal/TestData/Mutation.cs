namespace TSpec.Internal.TestData;

internal record Mutation<TValue>
{
    private readonly Func<TValue, int?, TValue> _transform;
    private bool _isApplied = false;

    public static implicit operator Mutation<TValue>(Func<TValue, TValue> transform) => new(transform);
    public static implicit operator Mutation<TValue>(Func<TValue, int, TValue> transform) => new(transform);
    public static implicit operator Mutation<TValue>(Action<TValue> setup) => new(setup);
    public static implicit operator Mutation<TValue>(Action<TValue, int> setup) => new(setup);

    private Mutation(Func<TValue, TValue> transform) => _transform = (val, i) => transform(val);

    private Mutation(Func<TValue, int, TValue> transform) => _transform = (val, i) => transform(val, i!.Value);

    private Mutation(Action<TValue, int> setup)
        => _transform = (val, i) =>
        {
            setup(val, i!.Value);
            return val;
        };

    private Mutation(Action<TValue> setup)
        => _transform = (val, i) =>
        {
            setup(val);
            return val;
        };

    internal TValue Apply(TValue value, int? index)
    {
        if (_isApplied)
            return value;

        _isApplied = true;
        return _transform(value, index);
    }
}