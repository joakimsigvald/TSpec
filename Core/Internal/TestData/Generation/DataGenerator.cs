using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Internal.TestData.Generation.Strategies.Mocking;

namespace TSpec.Internal.TestData.Generation;

internal class DataGenerator(
    Counter counter,
    TypeConversionStrategy typeConversionStrategy,
    DefaultStrategy defaultStrategy,
    MockingStrategy mockingStrategy,
    DisposalTracker disposalTracker)
{
    private static readonly StackStrategy _stackStrategy = new();
    private static readonly NullableStrategy _nullableStrategy = new();
    private static readonly EnumStrategy _enumStrategy = new();
    private static readonly CollectionStrategy _collectionStrategy = new();
    private static readonly ObjectStrategy _objectStrategy = new();

    private readonly IGenerationStrategy[] _strategies = [
        typeConversionStrategy,
        defaultStrategy,
        _nullableStrategy,
        _enumStrategy,
        new PrimitiveStrategy(counter),
        new SemanticTypeStrategy(counter),
        _collectionStrategy,
        _stackStrategy,
        mockingStrategy,
        _objectStrategy,
        ];

    internal TValue Create<TValue>(For scope) => (TValue)Create(typeof(TValue), scope)!;
    internal object? Create(Type type, For scope) => Create(new GenerationRequest(type, true, [], this, scope));
    internal object? CreateNew(Type type, For scope) => Create(new GenerationRequest(type, false, [], this, scope));

    internal object? Create(GenerationRequest request)
    {
        object? val = null;
        foreach (var strategy in _strategies)
            if (strategy.TryGenerate(request, ref val))
            {
                if (ShouldBeAutoDisposed(strategy))
                    disposalTracker.Track(val);
                return val;
            }
        return val;

        // Objects TSpec constructs for the subject-under-test graph are TSpec-owned
        // and disposed on teardown. Values from earlier strategies (user-provided
        // defaults, mocks, converted values) are not owned by TSpec.
        bool ShouldBeAutoDisposed(IGenerationStrategy strategy) => ReferenceEquals(strategy, _objectStrategy) && request.Scope.HasFlag(For.Subject);
    }
}