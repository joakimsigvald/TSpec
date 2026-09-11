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
    /// Mock the return-value given two input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2>(
        Func<TArg1, TArg2, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

    /// <summary>
    /// Mock the return-value given three input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3>(
        Func<TArg1, TArg2, TArg3, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

    /// <summary>
    /// Mock the return-value given four input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4>(
        Func<TArg1, TArg2, TArg3, TArg4, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

    /// <summary>
    /// Mock the return-value given five input parameters
    /// </summary>
    /// <param name="returns">A function providing the return value, given the arguments passed to the mocked method. The lambda signature must match the mocked call</param>
    /// <param name="returnsExpr">Captured automatically by the compiler — do not provide</param>
    /// <typeparam name="TArg1">The type of the first argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg2">The type of the second argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg3">The type of the third argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg4">The type of the fourth argument passed to the mocked method</typeparam>
    /// <typeparam name="TArg5">The type of the fifth argument passed to the mocked method</typeparam>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenThatReturnsContinuation<TSUT, TResult, TService, TReturns> Returns<TArg1, TArg2, TArg3, TArg4, TArg5>(
        Func<TArg1, TArg2, TArg3, TArg4, TArg5, TReturns> returns,
        [CallerArgumentExpression(nameof(returns))] string? returnsExpr = null);

}