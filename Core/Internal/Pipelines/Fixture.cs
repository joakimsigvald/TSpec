using Moq;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.TestData.Generation.Strategies;

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
    private protected Command? _methodUnderTest;

    protected Fixture()
    {
        _fixture = new(this);
        _context = new(this, _disposalTracker);
        Specification = SpecificationContext.Create();
    }

    public SpecificationContext Specification { get; init; }

    public void TearDown()
    {
        try
        {
            if (_fixture.IsSetUp)
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
                SpecificationContext.Release();
            }
        }
    }

    internal void SetDefault<TModel>(
        Action<TModel> setup, string setupExpr) where TModel : class
    {
        Specification.AddGiven<TModel>(setupExpr, false);
        AssertIsNotSetUp();
        _context.SetDefault(setup);
    }

    internal void SetDefault<TValue>(
        Func<TValue, TValue> transform, string transformExpr)
    {
        Specification.AddGiven<TValue>(transformExpr, false);
        AssertIsNotSetUp();
        _context.SetDefault(transform);
    }

    internal void SetDefault<TValue>(TValue defaultValue, For scope, string defaultValuesExpr)
    {
        if (!string.IsNullOrEmpty(defaultValuesExpr))
            Specification.AddGiven(defaultValuesExpr, scope);
        AssertIsNotSetUp();
        _context.Use(defaultValue, scope);
    }

    internal void Using<TValue>(TValue defaultValue, For scope, string defaultValuesExpr)
    {
        if (!string.IsNullOrEmpty(defaultValuesExpr))
            Specification.AddUsing(defaultValuesExpr, scope);
        AssertIsNotSetUp();
        _context.Use(defaultValue, scope);
    }

    internal void Using<TValue>(Func<TValue> defaultFactory, For scope, string defaultFactoryExpr)
    {
        if (!string.IsNullOrEmpty(defaultFactoryExpr))
            Specification.AddUsing(defaultFactoryExpr, scope);
        AssertIsNotSetUp();
        _context.Use(defaultFactory, scope);
    }

    internal void PrependSetUp(Delegate setUp, string setUpExpr)
    {
        AssertIsNotSetUp();
        _fixture.After(new(setUp ?? throw new SetupFailed("SetUp cannot be null"), setUpExpr));
    }

    internal void SetTearDown(Delegate tearDown, string tearDownExpr)
    {
        AssertIsNotSetUp();
        _fixture.Before(new(tearDown ?? throw new SetupFailed("TearDown cannot be null"), tearDownExpr));
    }

    internal Lazy<TSUT> Arrange()
    {
        _arranger.Arrange();
        return new Lazy<TSUT>(Instantiate<TSUT>);
    }

    internal TClass Instantiate<TClass>() => _context.Instantiate<TClass>();

    internal TClass InstantiateNew<TClass>() => _context.InstantiateNew<TClass>();

    internal Mock<TObject> GetMock<TObject>() where TObject : class
        => _context.GetMock<TObject>();

    internal void AppendUsing(Action given)
    {
        AssertIsNotSetUp();
        _arranger.AppendUsing(given);
    }

    internal void PrependGiven(Action given)
    {
        AssertIsNotSetUp();
        _arranger.PrependGiven(given);
    }

    internal void AppendGiven(Action given)
    {
        AssertIsNotSetUp();
        _arranger.AppendGiven(given);
    }

    internal void SetupThrows<TService>(Func<Exception> expected)
    {
        AssertIsNotSetUp();
        _context.SetupThrows<TService>(expected);
    }

    internal void Register<TTarget, TSource>(Func<TSource, TTarget>? convert, For scope, SequenceHolder sequence)
    {
        AssertIsNotSetUp();
        _context.Register(convert, scope, sequence);
    }

    internal void SetSequence(SequenceHolder sequence, Func<object?> next)
    {
        AssertIsNotSetUp();
        sequence.Next = next;
    }

    private void AssertIsNotSetUp()
    {
        if (_fixture.IsSetUp)
            throw new SetupFailed("Cannot provide setup after pipeline is set up");
    }
}