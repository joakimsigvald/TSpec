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
}