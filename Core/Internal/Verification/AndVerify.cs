using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Pipelines;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Verification;

internal class AndVerify<TSUT, TResult> : AndThen<TSUT, TResult>, IAndVerify<TResult>
{
    internal AndVerify(TestResult<TSUT, TResult> parent) : base(parent) { }

    /// <summary>
    /// Continuation to verify how many times the mocked service was invoked in aggregate, any method or property access
    /// </summary>
    public IAndVerify<TResult> And<TObject>(Ignore _ = default, Times? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyInvoked(MockTarget<TObject>.Family, Require(wasInvoked), wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify how many times a named method of the mocked service was invoked, ignoring arguments
    /// </summary>
    public IAndVerify<TResult> And<TObject>(string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyInvoked(MockTarget<TObject>.Family, method, wasInvoked, wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked
    /// </summary>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyCall(MockTarget<TObject>.Family, expression, null, expressionExpr!, null);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked a number of times
    /// </summary>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyCall(MockTarget<TObject>.Family, expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked and returned a value
    /// </summary>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyCall(MockTarget<TObject>.Family, expression, null, expressionExpr!, null);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked and returned a value a number of times
    /// </summary>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyCall(MockTarget<TObject>.Family, expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
    }

    public IAndVerify<TResult> And<TObject>(Func<TObject> mock, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
        => AndInvoked(MockTarget<TObject>.Of(mock, mockExpr!), wasInvoked, wasInvokedExpr);

    public IAndVerify<TResult> And<TObject>(Func<TObject> mock, string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
        => AndInvoked(MockTarget<TObject>.Of(mock, mockExpr!), method, wasInvoked, wasInvokedExpr);

    public IAndVerify<TResult> And<TObject>(
        Func<TObject> mock,
        Expression<Action<TObject>> expression,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
        => AndCalled(MockTarget<TObject>.Of(mock, mockExpr!), expression, null, expressionExpr, null);

    public IAndVerify<TResult> And<TObject>(
        Func<TObject> mock,
        Expression<Action<TObject>> expression,
        Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
        => AndCalled(MockTarget<TObject>.Of(mock, mockExpr!), expression, wasInvoked, expressionExpr, wasInvokedExpr);

    public IAndVerify<TResult> And<TObject>(Tag<TObject> mock, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
        => AndInvoked(MockTarget<TObject>.Of(mock, mockExpr!), wasInvoked, wasInvokedExpr);

    public IAndVerify<TResult> And<TObject>(Tag<TObject> mock, string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
        => AndInvoked(MockTarget<TObject>.Of(mock, mockExpr!), method, wasInvoked, wasInvokedExpr);

    public IAndVerify<TResult> And<TObject>(
        Tag<TObject> mock,
        Expression<Action<TObject>> expression,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
        => AndCalled(MockTarget<TObject>.Of(mock, mockExpr!), expression, null, expressionExpr, null);

    public IAndVerify<TResult> And<TObject>(
        Tag<TObject> mock,
        Expression<Action<TObject>> expression,
        Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
        => AndCalled(MockTarget<TObject>.Of(mock, mockExpr!), expression, wasInvoked, expressionExpr, wasInvokedExpr);

    private IAndVerify<TResult> AndInvoked<TObject>(MockTarget<TObject> target, Times wasInvoked, string? wasInvokedExpr)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyInvoked(target, wasInvoked, wasInvokedExpr!);
    }

    private IAndVerify<TResult> AndInvoked<TObject>(
        MockTarget<TObject> target, string method, Times wasInvoked, string? wasInvokedExpr)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyInvoked(target, method, wasInvoked, wasInvokedExpr!);
    }

    private IAndVerify<TResult> AndCalled<TObject>(
        MockTarget<TObject> target, LambdaExpression call, Times? wasInvoked, string? callExpr, string? wasInvokedExpr)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return _parent.VerifyCall(target, call, wasInvoked, callExpr!, wasInvokedExpr);
    }

    private static Times Require(Times? wasInvoked) => wasInvoked ?? throw MissingWasInvoked;


    private static TSpec.SetupFailed MissingWasInvoked
        => new("And<TService>() requires a 'wasInvoked' argument, e.g. And<TService>(wasInvoked: Never)");
}
