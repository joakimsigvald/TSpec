using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.TestData.Generation.Strategies;
using TSpec.Internal.TestData.Generation.Strategies.Mocking;

namespace TSpec.Internal.Pipelines;

internal interface ISpecificationProvider
{
    SpecificationContext Specification { get; }
}

internal abstract class Fixture<TSUT> : ISpecificationProvider
{
    private protected readonly Context _context = null!;
    private protected readonly SpecFixture<TSUT> _fixture = null!;
    private protected readonly Arranger _arranger = new();
    private protected readonly DisposalTracker _disposalTracker = new();
    private protected readonly PipelinePhase _phase = new();
    private protected Command? _methodUnderTest;

    protected Fixture()
    {
        _fixture = new(this);
        _context = new(this, _disposalTracker, _phase);
        Specification = SpecificationContext.Create();
    }

    public SpecificationContext Specification { get; init; }

    public void TearDown()
    {
        try
        {
            if (_phase.Current >= Phase.Act)
                _fixture.Dispose();
        }
        finally
        {
            try
            {
                _disposalTracker.DisposeAll();
            }
            finally
            {
                Specification.Release();
            }
        }
    }

    internal void SetDefault<TModel>(
        Action<TModel> setup, string setupExpr, For scope) where TModel : class
    {
        Specification.AddUsingSetup<TModel>(setupExpr, scope);
        AssertActHasNotBegun();
        _context.SetDefault(setup, scope);
    }

    internal void SetDefault<TValue>(
        Func<TValue, TValue> transform, string transformExpr, For scope)
    {
        Specification.AddUsingSetup<TValue>(transformExpr, scope);
        AssertActHasNotBegun();
        _context.SetDefault(transform, scope);
    }

    internal void SetDefault<TValue>(TValue defaultValue, For scope, string defaultValuesExpr)
    {
        if (!string.IsNullOrEmpty(defaultValuesExpr))
            Specification.AddGiven(defaultValuesExpr, scope);
        AssertActHasNotBegun();
        _context.Use(defaultValue, scope);
    }

    internal void Using<TValue>(TValue defaultValue, For scope, string defaultValuesExpr, bool owned = false)
    {
        if (!string.IsNullOrEmpty(defaultValuesExpr))
            Specification.AddUsing<TValue>(defaultValuesExpr, scope, owned);
        AssertActHasNotBegun();
        if (owned)
            _disposalTracker.Track(defaultValue);
        _context.Use(defaultValue, scope);
    }

    internal void Using<TValue>(Func<TValue> defaultFactory, For scope, string defaultFactoryExpr, bool owned = false)
    {
        if (!string.IsNullOrEmpty(defaultFactoryExpr))
            Specification.AddUsing<TValue>(defaultFactoryExpr, scope, owned);
        AssertActHasNotBegun();
        _context.Use(owned ? TrackCreated(defaultFactory) : defaultFactory, scope);
    }

    private Func<TValue> TrackCreated<TValue>(Func<TValue> factory) => () =>
    {
        var value = factory();
        _disposalTracker.Track(value);
        return value;
    };

    internal void PrependSetUp(Delegate setUp, string setUpExpr)
    {
        AssertActHasNotBegun();
        _fixture.PrependSetUp(new(setUp ?? throw new SetupFailed("SetUp cannot be null"), setUpExpr));
    }

    internal void SetTearDown(Delegate tearDown, string tearDownExpr)
    {
        AssertActHasNotBegun();
        _fixture.AppendTearDown(new(tearDown ?? throw new SetupFailed("TearDown cannot be null"), tearDownExpr));
    }

    internal TSUT Arrange()
    {
        _phase.AdvanceTo(Phase.Arrange);
        _arranger.Arrange();
        return Instantiate<TSUT>();
    }

    internal TClass Instantiate<TClass>() => _context.Instantiate<TClass>();

    internal TClass InstantiateNew<TClass>() => _context.InstantiateNew<TClass>();

    internal MockHandle GetMock<TObject>() where TObject : class
        => _context.GetMock<TObject>();

    internal void SetupReturnsDefault<TService, TReturns>(TReturns value)
        => _context.SetupReturnsDefault<TService, TReturns>(value);

    internal void AppendUsing(Action given)
    {
        AssertActHasNotBegun();
        _arranger.AppendUsing(given);
    }

    internal void PrependGiven(Action given)
    {
        AssertActHasNotBegun();
        _arranger.PrependGiven(given);
    }

    internal void AppendGiven(Action given)
    {
        AssertActHasNotBegun();
        _arranger.AppendGiven(given);
    }

    internal void SetupThrows<TService>(Func<Exception> expected)
    {
        AssertActHasNotBegun();
        _context.SetupThrows<TService>(expected);
    }

    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
    {
        AssertActHasNotBegun();
        _context.Register(convert, scope, sequence);
    }

    internal void SetSequence(SequenceHolder sequence, Func<object?> next)
    {
        AssertActHasNotBegun();
        sequence._next = next;
    }

    private void AssertActHasNotBegun()
    {
        if (_phase.Current >= Phase.Act)
            throw new SetupFailed("Cannot provide setup after pipeline is set up");
    }
}