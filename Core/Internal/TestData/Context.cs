using TSpec.Internal.Mocking;
using TSpec.Internal.Specification;
using TSpec.Internal.Pipelines;
using TSpec.Internal.TestData.Generation.Strategies;

namespace TSpec.Internal.TestData;

internal class Context(
    ISpecificationProvider specificationProvider,
    DisposalTracker disposalTracker,
    IPipelinePhase phase,
    SetupLambda setupLambda)
{
    private readonly Repository _repository = new(specificationProvider, disposalTracker, phase, setupLambda);
    private readonly Dictionary<Type, Dictionary<object, int>> _tagIndices = [];
    private readonly HashSet<string> _tagNames = new(StringComparer.Ordinal);
    private readonly HashSet<(Type, int)> _readWhileDeclaring = [];
    private readonly Dictionary<(Type, int), string> _namesByIndex = [];

    /// <summary>
    /// A read the test made before the pipeline was arranged. Harmless on its own — it generates a
    /// value and keeps it — but wrong the moment an arrangement replaces it, because what the test is
    /// holding is then not what the pipeline used.
    /// </summary>
    private TValue Read<TValue>(int? index)
    {
        if (index is not null)
            NoteRead(typeof(TValue), index.Value);
        return Produce<TValue>(index);
    }

    private void NoteRead(Type type, int index)
    {
        if (phase.Current == Phase.Declare)
            _readWhileDeclaring.Add((type, index));
    }

    /// <summary>
    /// Raised where the arrangement lands, which is the first moment TSpec can tell the earlier read
    /// was wrong. Only a REPLACEMENT is wrong: an arrangement built from what was read stores it back
    /// unchanged — <c>Given(Many&lt;T&gt;())</c> — and one that mutates a value in place leaves the
    /// read reference pointing at the arranged object, so both keep the test holding the right thing.
    /// </summary>
    private void AssertNotReplacingWhatWasRead(Type type, int index, object? value)
    {
        if (!WasReadBeforeArrange(type, index))
            return;

        var (prior, found) = _repository.Retrieve(type, index);
        if (!found || Equals(prior, value))
            return;

        throw new SetupFailed(
            $"{NameOf(type, index)} was read before the pipeline was arranged, so the read yielded a "
            + "generated value rather than the one arranged for it. "
            + "Run the pipeline with Then() before reading it");
    }

    private bool WasReadBeforeArrange(Type type, int index) 
        => (phase.Current is Phase.Arrange or Phase.Mock) && _readWhileDeclaring.Contains((type, index));

    private string NameOf(Type type, int index)
        => _namesByIndex.TryGetValue((type, index), out var name)
            ? $"the tag '{name}'"
            : $"the {Ordinal(index)} {type.Alias()}";

    private static string Ordinal(int index) => index switch
    {
        0 => "first",
        1 => "second",
        2 => "third",
        3 => "fourth",
        4 => "fifth",
        _ => $"#{index + 1}"
    };

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
                : _repository.TryResolveDefault(typeof(TValue), For.Input, out var defaultValue)
                ? (TValue)defaultValue!
                : Create<TValue>();
            if (mutation is null)
                return newValue;
            try
            {
                return setupLambda.Run(() => mutation.Apply(newValue, index));
            }
            catch (Exception ex) when (ex is not SetupFailed)
            {
                throw new SetupFailed("Failed to apply transform", ex);
            }
        }
    }

    internal TValue Mention<TValue>(int? index) => Read<TValue>(index);

    internal TValue Mention<TValue>(Tag<TValue> tag) => Read<TValue>(GetTagIndex(tag));

    internal TValue Assign<TValue>(Tag<TValue> tag, TValue value) => Assign(value, GetTagIndex(tag));

    internal TValue Apply<TValue>(Tag<TValue> tag, Mutation<TValue> mutation)
        => Apply(mutation, GetTagIndex(tag));

    internal Dictionary<object, int> GetTagIndices(Type type)
        => _tagIndices.TryGetValue(type, out var val) ? val : _tagIndices[type] = [];

    internal void SetDefault<TModel>(Action<TModel> setup, For scope) where TModel : class
        => _repository.AddDefaultSetup(
            typeof(TModel),
            scope,
            RunAsSetupLambda(obj =>
            {
                if (obj is TModel model)
                    setup(model);
                return obj;
            }));

    internal void SetDefault<TValue>(Func<TValue, TValue> setup, For scope)
        => _repository.AddDefaultSetup(typeof(TValue), scope, RunAsSetupLambda(_ => setup((TValue)_)!));

    private Func<object, object> RunAsSetupLambda(Func<object, object> setup)
        => obj => setupLambda.Run(() => setup(obj));

    internal TValue[] AssignMany<TValue>(TValue[] values)
        => Assign(values);

    /// Every stored value passes here, which is the one place a replacement can be caught.
    internal TValue Assign<TValue>(TValue value, int index = 0)
    {
        AssertNotReplacingWhatWasRead(typeof(TValue), index, value);
        _repository.Assign(typeof(TValue), value, index, () => MentionName(typeof(TValue), index));
        return value;
    }

    /// A mention as the specification words it: the IRule, the second IRule, the Primary.
    private string MentionName(Type type, int index)
        => _namesByIndex.TryGetValue((type, index), out var tag) ? $"the {tag.AsTagName()}"
        : index == 0 ? $"the {type.Alias()}"
        : $"the {Ordinal(index)} {type.Alias()}";

    internal TValue[] MentionMany<TValue>(int count, int? minCount)
    {
        NoteRead(typeof(TValue[]), 0);
        return Assign(Reuse(GetArray<TValue>(), count, minCount));
    }

    private TValue[]? GetArray<TValue>()
    {
        var type = typeof(TValue[]);
        var (val, found) = _repository.Retrieve(type);
        return (found || _repository.TryResolveDefault(type, For.Input, out val)) ? val as TValue[] : null;
    }

    internal TValue[] ApplyMany<TValue>(Mutation<TValue> mutation, int count)
        => Assign(Enumerable.Range(0, count).Select(i => Apply(mutation with { }, i)).ToArray());

    internal TValue Create<TValue>() => _repository.Create<TValue>(For.Input);

    internal TValue Create<TValue>(Action<TValue> setup)
    {
        var value = Create<TValue>();
        return setupLambda.Run(() =>
        {
            setup(value);
            return value;
        });
    }

    internal MockFamily GetMockFamily<TObject>() where TObject : class
        => _repository.GetMockFamily<TObject>();

    internal MockHandle MockOf(object? value, string mentionName) => _repository.MockOf(value, mentionName);

    internal void Use<TService>(TService service, For scope) => _repository.Use(service, scope);
    internal void Use<TService>(Func<TService> factory, For scope) => _repository.Use(factory, scope);

    internal void SetupThrows<TService>(Func<Exception> ex)
        => _repository.SetDefaultException(typeof(TService), ex);

    internal void SetupReturnsDefault<TService, TReturns>(TReturns value)
        => _repository.SetProvidedDefault(typeof(TService), typeof(TReturns), value);

    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
        => _repository.Register(convert, scope, sequence);

    /// <summary>
    /// A tag is registered under its own name the first time the test touches it, and the name has
    /// to be unique within the test — it is how a value is identified in a failure report, and two
    /// values labelled alike identify nothing.
    /// </summary>
    private int GetTagIndex<TValue>(Tag<TValue> tag)
    {
        var type = typeof(TValue);
        var typedTagIndices = GetTagIndices(type);
        if (typedTagIndices.TryGetValue(tag, out var index))
            return index;

        AssertNameIsUnique(tag.Name);
        index = GetNextTagIndex(typedTagIndices);
        specificationProvider.Specification.TagIndex(type, index, tag.Name);
        _namesByIndex[(type, index)] = tag.Name;
        return typedTagIndices[tag] = index;
    }

    /// <summary>
    /// A tag takes the name of the variable it is assigned to, which the compiler can only supply
    /// for a field — tags declared inside a method all take that method's name, and so collide.
    /// </summary>
    private void AssertNameIsUnique(string tagName)
    {
        if (_tagNames.Add(tagName))
            return;

        throw new SetupFailed(
            $"Two tags are named '{tagName}'. A tag is named after the variable it is assigned to, "
            + "but the compiler can only see that in a field declaration — tags declared inside a "
            + "method take the method's name instead. Name each one where it is declared, "
            + "for example: Tag<int> low = new(nameof(low));");
    }

    private static int GetNextTagIndex(Dictionary<object, int> typedTagIndices)
        => typedTagIndices.Count > 0 ? typedTagIndices.Values.Min() - 1 : -1;

    private TValue[] Reuse<TValue>(TValue[]? arr, int count, int? minCount)
        => arr is null ? [.. Enumerable.Range(0, count).Select(i => Read<TValue>(i))]
        : arr.Length >= minCount || arr?.Length == count ? arr
        : arr!.Length > count ? arr[..count]
        : Extend(arr, count);

    private TValue[] Extend<TValue>(TValue[] arr, int count)
        => [
            .. arr,
            .. Enumerable.Range(arr.Length, count - arr.Length).Select(i => Read<TValue>(i))
            ];
}