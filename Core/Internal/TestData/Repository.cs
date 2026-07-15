using Moq;
using TSpec.Internal.Pipelines;
using TSpec.Internal.TestData.Generation;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Internal.TestData.Generation.Strategies.Mocking;

namespace TSpec.Internal.TestData;

internal class Repository : IRepository
{
    private readonly Mutator _mutator = new();
    private readonly TypeConversionStrategy _typeConversionStrategy = new();
    private readonly DataProvider _dataProvider;
    private readonly MockingStrategy _mockingStrategy;
    private readonly DataGenerator _generator;
    private readonly FluentDefaultProvider _fluentDefaultProvider;
    private readonly Dictionary<Type, Dictionary<int, object?>> _numberedMentions = [];
    private readonly ISpecificationProvider _specificationProvider;

    public Repository(ISpecificationProvider specificationProvider)
    {
        _dataProvider = new();
        _fluentDefaultProvider = new(this);
        _mockingStrategy = new(_fluentDefaultProvider);
        _generator = new(new(), _typeConversionStrategy, new(this), _mockingStrategy);
        _specificationProvider = specificationProvider;
    }

    internal (object? val, bool found) Retrieve(Type type, int index = 0)
        => _numberedMentions.TryGetValue(type, out var map) && map.TryGetValue(index, out var val)
            ? (val, found: true)
            : (null, found: false);

    internal void Assign(Type type, object? value, int index)
    {
        _specificationProvider.Specification.Assign(type, index, value);
        GetMentions(type)[index] = value;
    }

    internal object? Instantiate<TValue>()
    {
        var type = typeof(TValue);
        var instance = _generator.Create(type, For.Subject);
        return _mutator.Mutate(type, instance);
    }

    internal object? InstantiateNew<TValue>()
    {
        var type = typeof(TValue);
        var instance = _generator.CreateNew(type, For.Subject);
        return _mutator.Mutate(type, instance);
    }

    public bool TryGetDefault(Type type, For scope, out object? val)
    {
        var found = _dataProvider.TryGetValue(type, scope, out val);
        if (found)
            return true;

        if (!_mutator.HasMutation(type))
            return false;

        val = _mutator.Mutate(type, _generator.CreateNew(type, scope));
        return true;
    }

    internal void Use<TValue>(TValue value, For scope) => _dataProvider.UseValue(value, scope);

    internal void Use<TValue>(Func<TValue> factory, For scope)
    {
        Use<Func<TValue>>(factory, scope);
        _dataProvider.UseFactory(factory, scope);
    }

    internal void AddDefaultSetup(Type type, Func<object, object> mutation) => _mutator.AddMutation(type, mutation);

    public (object? val, bool found) Use(Type type, For scope)
        => TryGetDefault(type, scope, out var value) ? (value, true) : (null, false);

    public object Create(Type type, For scope) => _mutator.Mutate(type, _generator.Create(type, scope))!;

    internal TValue Create<TValue>(For scope)
        => (TValue)_mutator.Mutate(typeof(TValue), _generator.Create<TValue>(scope))!;


    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
        => _typeConversionStrategy.Register(convert, scope, sequence);

    internal Mock GetMock(Type type) => _mockingStrategy.GetMock(type);

    internal Mock<TObject> GetMock<TObject>() where TObject : class => _mockingStrategy.GetMock<TObject>();

    internal void SetDefaultException(Type type, Func<Exception> ex)
        => _fluentDefaultProvider.SetDefaultException(type, ex);

    private Dictionary<int, object?> GetMentions(Type type)
        => _numberedMentions.TryGetValue(type, out var val) ? val : _numberedMentions[type] = [];
}