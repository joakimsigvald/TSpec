using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation for chaining further arrangement after the outcome of a mocked method invocation has been specified
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
/// <typeparam name="TReturns">The return type of the mocked method invocation</typeparam>
public interface IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> : IGivenTestPipeline<TSUT, TResult>
    where TService : class
{
    /// <summary>
    /// Setup mock to return a value as default for any invocation where no specific mock-setup has been provided
    /// </summary>
    /// <typeparam name="TReturns2">The return type to provide a default value for</typeparam>
    /// <param name="value">A function providing the default value to return</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> AndReturnsDefault<TReturns2>(Func<TReturns2> value);

    /// <summary>
    /// Mock another method invocation on the same service
    /// </summary>
    /// <typeparam name="TReturns2">The return type of the mocked method invocation</typeparam>
    /// <param name="call">An expression specifying the method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, TReturns2>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);

    /// <summary>
    /// Mock another async method invocation on the same service
    /// </summary>
    /// <typeparam name="TReturns2">The return type of the mocked async method invocation</typeparam>
    /// <param name="call">An expression specifying the async method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns2> AndThat<TReturns2>(
        Expression<Func<TService, Task<TReturns2>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);

    /// <summary>
    /// Returns a continuation for providing the next mocked result of a sequence of method invocations.
    /// </summary>
    /// <returns>A continuation for specifying the outcome of the next invocation in the sequence</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> AndNext();
}