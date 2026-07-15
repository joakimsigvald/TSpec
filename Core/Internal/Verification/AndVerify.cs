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
    /// Continuation to verify a mock was invoked
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="expression"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
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
    /// <typeparam name="TObject"></typeparam>
    /// <param name="expression"></param>
    /// <param name="times"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null) 
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, times, expressionExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked a number of times
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="expression"></param>
    /// <param name="times"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null) 
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, times, expressionExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked and returned a value
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TReturns"></typeparam>
    /// <param name="expression"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
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
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TReturns"></typeparam>
    /// <param name="expression"></param>
    /// <param name="times"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, times, expressionExpr!);
    }

    /// <summary>
    /// Continuation to verify a mock was invoked and returned a value a number of times
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TReturns"></typeparam>
    /// <param name="expression"></param>
    /// <param name="times"></param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class
    {
        SpecificationContext.Current.AddThen();
        return Parent.Verify(expression, times, expressionExpr!);
    }
}