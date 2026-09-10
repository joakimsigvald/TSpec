using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation for specifying the outcome of a mocked method invocation, as a return value or a thrown exception
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
/// <typeparam name="TReturns">The return type of the mocked method invocation</typeparam>
public interface IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns>
    where TService : class
{
    /// <summary>
    /// After using Tap to inspect or use incoming parameters of a mocked method invocation,
    /// call Returns (with or without return value) to complete the setup of the mock.
    /// Otherwise the mocked behavior of this method will not be applied when running the test pipeline.
    /// </summary>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns();

    /// <summary>
    /// Mock the return-value of a method invocation
    /// </summary>
    /// <param name="returns">A function providing the value to return from the mocked invocation</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Func<TReturns?> returns, [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

    /// <summary>
    /// Mock the return-value of a method invocation as the value associated with the given tag
    /// </summary>
    /// <param name="tag">The tag whose associated value to return from the mocked invocation</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns(
        Tag<TReturns?> tag, [CallerArgumentExpression(nameof(tag))] string? tagExpr = null);

    /// <summary>
    /// Mock the return-value as default
    /// </summary>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> ReturnsDefault();

    /// <summary>
    /// Setup mock to throw an exception of the given type from the mocked invocation
    /// </summary>
    /// <typeparam name="TException">The type of exception to throw from the mocked invocation</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws<TException>() where TException : Exception, new();

    /// <summary>
    /// Setup mock to throw the given exception from the mocked invocation
    /// </summary>
    /// <param name="expected">A function providing the exception to throw from the mocked invocation</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Throws(
        Func<Exception> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null);

    /// <summary>
    /// Provide a callback to observe the mocked call without deciding its outcome. Inside a
    /// sequence the tap belongs to the step it precedes, so it fires on the call that step answers;
    /// outside one it fires on every call. Follow it with Returns or Throws to complete the setup.
    /// </summary>
    /// <param name="callback">A callback invoked when the mocked method is called</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap(
        Action callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to observe the arguments of the mocked call without deciding its outcome.
    /// </summary>
    /// <typeparam name="TArg">The type of argument 1 passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg>(
        Action<TArg> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to observe the arguments of the mocked call without deciding its outcome.
    /// </summary>
    /// <typeparam name="TArg1">The type of argument 1 passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of argument 2 passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2>(
        Action<TArg1, TArg2> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to observe the arguments of the mocked call without deciding its outcome.
    /// </summary>
    /// <typeparam name="TArg1">The type of argument 1 passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of argument 2 passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of argument 3 passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3>(
        Action<TArg1, TArg2, TArg3> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to observe the arguments of the mocked call without deciding its outcome.
    /// </summary>
    /// <typeparam name="TArg1">The type of argument 1 passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of argument 2 passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of argument 3 passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of argument 4 passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4>(
        Action<TArg1, TArg2, TArg3, TArg4> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Provide a callback to observe the arguments of the mocked call without deciding its outcome.
    /// </summary>
    /// <typeparam name="TArg1">The type of argument 1 passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of argument 2 passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of argument 3 passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of argument 4 passed to the mocked method</typeparam>
    /// <typeparam name="TArg5">The type of argument 5 passed to the mocked method</typeparam>
    /// <param name="callback">A callback invoked with the arguments passed to the mocked method</param>
    /// <param name="callbackExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for specifying the outcome of the mocked invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> Tap<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Action<TArg1, TArg2, TArg3, TArg4, TArg5> callback,
        [CallerArgumentExpression(nameof(callback))] string? callbackExpr = null);

    /// <summary>
    /// Begin a sequence: this and each AndNext state the outcome of one successive call.
    /// Past the last step the call answers with the return type's default.
    /// </summary>
    /// <returns>A continuation for specifying the outcome of the first invocation</returns>
    IGivenThatCommonContinuation<TSUT, TResult, TService, TReturns> First();
}
