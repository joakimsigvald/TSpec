using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation for setting up calls on a mock: on every mock of a type, or on the one a mention names
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
/// <typeparam name="TService">The mocked type</typeparam>
public interface IGivenMockContinuation<TSUT, TResult, TService>
    where TService : class
{
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

    /// <summary>
    /// Provide async method invocation to mock
    /// </summary>
    /// <typeparam name="TReturns">The return type of the mocked async invocation</typeparam>
    /// <param name="call">An expression specifying the async method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(
        Expression<Func<TService, ValueTask<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null);
}