using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation object to apply additional arrangements to the test-pipeline
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
/// <example>
/// Mock a calculator to add the two arguments passed to it:
/// <code>
/// Given&lt;ICalculator&gt;()
///     .That(_ =&gt; _.Add(TheFirst&lt;int&gt;(), TheSecond&lt;int&gt;()))
///     .Returns((a, b) =&gt; a + b)
/// </code>
/// </example>
public interface IGivenServiceContinuation<TSUT, TResult, TService>
    where TService : class
{
    /// <summary>
    /// Setup mock to return a value as default for any invocation where no specific mock-setup has been provided
    /// </summary>
    /// <typeparam name="TReturns">The return type to provide a default value for</typeparam>
    /// <param name="value">A function providing the default value to return</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Func<TReturns> value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Setup mock to return a tagged value as default for any invocation where no specific mock-setup has been provided
    /// </summary>
    /// <typeparam name="TReturns">The return type to provide a default value for</typeparam>
    /// <param name="value">The tag whose associated value to return</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TReturns>(
        Tag<TReturns> value,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null);

    /// <summary>
    /// Setup mock to throw an exception for any call, unless otherwise specified
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Throws<TException>() where TException : Exception;

    /// <summary>
    /// Setup mock to throw the given exception for any call
    /// </summary>
    /// <param name="expected">A function providing the exception to throw</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null);

    /// <summary>
    /// Mock the void method invocation
    /// </summary>
    /// <param name="call">An expression specifying the method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatVoidContinuation<TSUT, TResult, TService> That(
        Expression<Action<TService>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);

    /// <summary>
    /// Mock the value-returning method invocation
    /// </summary>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="call">An expression specifying the method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, TReturns>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);

    /// <summary>
    /// Provide async method invocation to mock
    /// </summary>
    /// <typeparam name="TReturns">The return type of the mocked async invocation</typeparam>
    /// <param name="call">An expression specifying the async method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, Task<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);
}