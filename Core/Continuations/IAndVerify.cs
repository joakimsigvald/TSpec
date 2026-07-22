using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation to apply additional assertions on the test result
/// </summary>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface IAndVerify<TResult> : IAndThen<TResult>
{
    /// <summary>
    /// Continue with an aggregate invocation assertion on the given mocked service, e.g. And&lt;IEmailSender&gt;().WasInvoked(Never)
    /// </summary>
    /// <typeparam name="TObject">The mocked type to assert invocations on</typeparam>
    /// <returns>A continuation to assert on the aggregate invocations of the service</returns>
    public IVerifyService<TResult> And<TObject>() where TObject : class;

    /// <summary>
    /// Verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called.
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. nameof(IEventQueue.MarkFailed)</param>
    /// <param name="times">The number of times the method is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject>(string method, Times times,
        [CallerArgumentExpression(nameof(times))] string? timesExpr = null) where TObject : class;

    /// <summary>
    /// Verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called.
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. nameof(IEventQueue.MarkFailed)</param>
    /// <param name="times">A function providing the number of times the method is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject>(string method, Func<Times> times,
        [CallerArgumentExpression(nameof(times))] string? timesExpr = null) where TObject : class;

    /// <summary>
    /// Assert that a mock invocation satisfies the given expression
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;

    /// <summary>
    /// Assert that a mock invocation satisfying the given expression was made the given number of times
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;

    /// <summary>
    /// Assert that a mock invocation satisfying the given expression was made the number of times given by a function
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject>(
        Expression<Action<TObject>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;

    /// <summary>
    /// Assert that a value-returning mock invocation satisfies the given expression
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;

    /// <summary>
    /// Assert that a value-returning mock invocation satisfying the given expression was made the given number of times
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;

    /// <summary>
    /// Assert that a value-returning mock invocation satisfying the given expression was made the number of times given by a function
    /// </summary>
    /// <typeparam name="TObject">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    public IAndVerify<TResult> And<TObject, TReturns>(
        Expression<Func<TObject, TReturns>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TObject : class;
}