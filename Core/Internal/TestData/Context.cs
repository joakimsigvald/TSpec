using Moq;
using TSpec.Internal.Pipelines;
using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData;

internal class Context(ISpecificationProvider specificationProvider, DisposalTracker disposalTracker)
{
    private readonly Repository _repository = new(specificationProvider, disposalTracker);
    private readonly Dictionary<Type, Dictionary<object, int>> _tagIndices = [];

    internal TClass Instantiate<TClass>()
        => (TClass)(_repository.Instantiate<TClass>() ?? Create<TClass>())!;

    internal TClass InstantiateNew<TClass>()
        => (TClass)_repository.InstantiateNew<TClass>()!;

    internal TValue Apply<TValue>(Mutation<TValue>? mutation, int? index) => Produce(index, mutation);

    internal TValue Produce<TValue>(int? index, Mutation<TValue>? mutation = null)
    {
        if (index is null)
            return Create<TValue>();

        var (val, found) = _repository.Retrieve(typeof(TValue), index.Value);
        if (found && mutation is null)
            return (TValue)val!;
        return Assign(Get(), index.Value);

        TValue Get()
        {
            var newValue = found
                ? (TValue)val!
                : _repository.TryGetDefault(typeof(TValue), For.Input, out var defaultValue)
                ? (TValue)defaultValue!
                : Create<TValue>();
            if (mutation is null)
                return newValue;
            try
            {
                return mutation.Apply(newValue, index);
            }
            catch (Exception ex)
            {
                throw new SetupFailed("Failed to apply transform", ex);
            }
        }
    }

    internal TValue Produce<TValue>(Tag<TValue> tag, string tagName)
        => Produce<TValue>(GetTagIndex(tag, tagName));

    internal TValue Assign<TValue>(Tag<TValue> tag, TValue value, string tagName)
        => Assign(value, GetTagIndex(tag, tagName));

    internal TValue Apply<TValue>(Tag<TValue> tag, Mutation<TValue> mutation, string tagName)
        => Apply(mutation, GetTagIndex(tag, tagName));

    internal Dictionary<object, int> GetTagIndices(Type type)
        => _tagIndices.TryGetValue(type, out var val) ? val : _tagIndices[type] = [];

    internal void SetDefault<TModel>(Action<TModel> setup) where TModel : class
        => _repository.AddDefaultSetup(
            typeof(TModel),
            obj =>
            {
                if (obj is TModel model)
                    setup(model);
                return obj;
            });

    internal void SetDefault<TValue>(Func<TValue, TValue> setup)
        => _repository.AddDefaultSetup(typeof(TValue), _ => setup((TValue)_)!);

    internal TValue[] AssignMany<TValue>(TValue[] values)
        => Assign(values);

    internal TValue Assign<TValue>(TValue value, int index = 0)
    {
        _repository.Assign(typeof(TValue), value, index);
        return value;
    }

    internal TValue[] MentionMany<TValue>(int count, int? minCount)
        => Assign(Reuse(GetArray<TValue>(), count, minCount));

    private TValue[]? GetArray<TValue>()
    {
        var type = typeof(TValue[]);
        var (val, found) = _repository.Retrieve(type);
        return (found || _repository.TryGetDefault(type, For.Input, out val)) ? val as TValue[] : null;
    }

    internal TValue[] ApplyMany<TValue>(Mutation<TValue> mutation, int count)
        => Assign(Enumerable.Range(0, count).Select(i => Apply(mutation with { }, i)).ToArray());

    internal TValue Create<TValue>() => _repository.Create<TValue>(For.Input);

    internal Mock<TObject> GetMock<TObject>() where TObject : class
        => _repository.GetMock<TObject>();

    internal void Use<TService>(TService service, For scope) => _repository.Use(service, scope);
    internal void Use<TService>(Func<TService> factory, For scope) => _repository.Use(factory, scope);

    internal void SetupThrows<TService>(Func<Exception> ex)
        => _repository.SetDefaultException(typeof(TService), ex);

    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
        => _repository.Register(convert, scope, sequence);

    private int GetTagIndex<TValue>(Tag<TValue> tag, string tagName)
    {
        var type = typeof(TValue);
        var typedTagIndices = GetTagIndices(type);
        if (typedTagIndices.TryGetValue(tag, out var index))
            return index;

        index = GetNextTagIndex(typedTagIndices);
        specificationProvider.Specification.TagIndex(type, index, tagName);
        return typedTagIndices[tag] = index;
    }

    private static int GetNextTagIndex(Dictionary<object, int> typedTagIndices)
        => typedTagIndices.Count > 0 ? typedTagIndices.Values.Min() - 1 : -1;

    private TValue[] Reuse<TValue>(TValue[]? arr, int count, int? minCount)
        => arr is null ? [.. Enumerable.Range(0, count).Select(i => Produce<TValue>(i))]
        : arr.Length >= minCount || arr?.Length == count ? arr
        : arr!.Length > count ? arr[..count]
        : Extend(arr, count);

    private TValue[] Extend<TValue>(TValue[] arr, int count)
        => [
            .. arr,
            .. Enumerable.Range(arr.Length, count - arr.Length).Select(i => Produce<TValue>(i))
            ];
}