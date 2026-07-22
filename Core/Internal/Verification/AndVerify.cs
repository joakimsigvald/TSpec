using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Verification;

internal class AndVerify<TSUT, TResult> : AndThen<TSUT, TResult>, IAndVerify<TResult>
{
    internal AndVerify(TestResult<TSUT, TResult> parent) : base(parent) { }

    /// <summary>
    /// Continuation to assert on the aggregate invocations of a mocked service
    /// </summary>
    [Obsolete("Use And<TObject>(wasInvoked: Times) instead, e.g. And<IEmailSender>(wasInvoked: Never).")]
    public IVerifyService<TResult> And<TObject>() where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.VerifyService<TObject>();
    }

    /// <summary>
    /// Continuation to verify how many times the mocked service was invoked in aggregate, any method or property access
    /// </summary>
    public IAndVerify<TResult> And<TObject>(Ignore _ = default, Times? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.VerifyInvoked<TObject>(Require(wasInvoked), wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify how many times the mocked service was invoked in aggregate, any method or property access
    /// </summary>
    public IAndVerify<TResult> And<TObject>(Ignore _ = default, Func<Times>? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.VerifyInvoked<TObject>(Require(wasInvoked)(), wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify how many times a named method of the mocked service was invoked, ignoring arguments
    /// </summary>
    public IAndVerify<TResult> And<TObject>(string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.VerifyInvoked<TObject>(method, wasInvoked, wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify how many times a named method of the mocked service was invoked, ignoring arguments
    /// </summary>
    public IAndVerify<TResult> And<TObject>(string method, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.VerifyInvoked<TObject>(method, wasInvoked(), wasInvokedExpr!);
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
        return Parent.Verify(expression, expressionExpr!);
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
        return Parent.Verify(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked a number of times
    /// </summary>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
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
        return Parent.Verify(expression, expressionExpr!);
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
        return Parent.Verify(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked and returned a value a number of times
    /// </summary>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
    }

    private static Times Require(Times? wasInvoked) => wasInvoked ?? throw MissingWasInvoked;

    private static Func<Times> Require(Func<Times>? wasInvoked) => wasInvoked ?? throw MissingWasInvoked;

    private static TSpec.SetupFailed MissingWasInvoked
        => new("And<TService>() requires a 'wasInvoked' argument, e.g. And<TService>(wasInvoked: Never)");
}
