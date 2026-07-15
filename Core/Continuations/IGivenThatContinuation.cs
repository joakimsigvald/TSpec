using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation to mock the result of a method invocation
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
/// <typeparam name="TReturns">The return type of the mocked method invocation</typeparam>
public interface IGivenThatContinuation<TSUT, TResult, TService, TReturns>
    : IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    /// <summary>
    /// Provide a callback to tap the mocked function call with no input arguments
    /// </summary>
    /// <param name="callback">A callback invoked when the mocked method is called</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap(
        Action callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to tap the mocked function call with one input argument
    /// </summary>
    /// <typeparam name="TArg">The type of the argument passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the argument passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg>(
        Action<TArg> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Mock the return-value given one input parameter
    /// </summary>
    /// <param name="returns">A function providing the return value, given the argument passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <typeparam name="TArg">The type of the argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg>(
        Func<TArg, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

    /// <summary>
    /// Provide a callback to tap the mocked function call with two input arguments
    /// </summary>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2>(
        Action<TArg1, TArg2> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Mock the return-value given two input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2>(
        Func<TArg1, TArg2, TReturns> returns);

    /// <summary>
    /// Provide a callback to tap the mocked function call with three input arguments
    /// </summary>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3>(
        Action<TArg1, TArg2, TArg3> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Mock the return-value given three input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3>(
        Func<TArg1, TArg2, TArg3, TReturns> returns);

    /// <summary>
    /// Provide a callback to tap the mocked function call with four input arguments
    /// </summary>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4>(
        Action<TArg1, TArg2, TArg3, TArg4> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Mock the return-value given four input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4>(
        Func<TArg1, TArg2, TArg3, TArg4, TReturns> returns);

    /// <summary>
    /// Provide a callback to tap the mocked function call with five input arguments
    /// </summary>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg5">The type of the fifth argument passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Action<TArg1, TArg2, TArg3, TArg4, TArg5> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Mock the return-value given five input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg5">The type of the fifth argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TReturns> returns);

    /// <summary>
    /// Returns a continuation for providing the first mocked result of a sequence of method invocations.
    /// </summary>
    /// <returns>A continuation for specifying the outcome of the first invocation in the sequence</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> First();
}