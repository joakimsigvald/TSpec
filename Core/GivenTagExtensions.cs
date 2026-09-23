using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Pipelines;

namespace TSpec;

/// <summary>
/// Setting up the one mock a tag holds: <c>Given(primary).That(_ =&gt; _.Passes()).Returns(() =&gt; false)</c>.
/// Extensions, since only a tag of a mocked type can be set up.
/// </summary>
public static class GivenTagExtensions
{
    /// <summary>
    /// Mock the void method invocation on the tagged mock
    /// </summary>
    /// <param name="given">The tag of the mock to set up</param>
    /// <param name="call">An expression specifying the method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    public static IGivenThatVoidContinuation<TSUT, TResult, TMock> That<TSUT, TResult, TMock>(
        this IGivenTag<TSUT, TResult, TMock> given,
        Expression<Action<TMock>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        where TMock : class
        => MockOf(given).That(call, callExpr!);

    /// <summary>
    /// Mock the value-returning method invocation on the tagged mock
    /// </summary>
    /// <param name="given">The tag of the mock to set up</param>
    /// <param name="call">An expression specifying the method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    public static IGivenThatContinuation<TSUT, TResult, TMock, TReturns> That<TSUT, TResult, TMock, TReturns>(
        this IGivenTag<TSUT, TResult, TMock> given,
        Expression<Func<TMock, TReturns>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        where TMock : class
        => MockOf(given).That(call, callExpr!);

    /// <summary>
    /// Mock the async method invocation on the tagged mock
    /// </summary>
    /// <param name="given">The tag of the mock to set up</param>
    /// <param name="call">An expression specifying the async method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    public static IGivenThatContinuation<TSUT, TResult, TMock, TReturns> That<TSUT, TResult, TMock, TReturns>(
        this IGivenTag<TSUT, TResult, TMock> given,
        Expression<Func<TMock, Task<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        where TMock : class
        => MockOf(given).That(call, callExpr!);

    /// <summary>
    /// Mock the async method invocation on the tagged mock
    /// </summary>
    /// <param name="given">The tag of the mock to set up</param>
    /// <param name="call">An expression specifying the async method invocation to mock</param>
    /// <param name="callExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing the method invocation result to mock</returns>
    public static IGivenThatContinuation<TSUT, TResult, TMock, TReturns> That<TSUT, TResult, TMock, TReturns>(
        this IGivenTag<TSUT, TResult, TMock> given,
        Expression<Func<TMock, ValueTask<TReturns>>> call,
        [CallerArgumentExpression(nameof(call))] string? callExpr = null)
        where TMock : class
        => MockOf(given).That(call, callExpr!);

    private static GivenServiceContinuation<TSUT, TResult, TMock> MockOf<TSUT, TResult, TMock>(
        IGivenTag<TSUT, TResult, TMock> given)
        where TMock : class
    {
        var tagged = (GivenTag<TSUT, TResult, TMock>)given;
        return tagged.Mock(tagged.Tag);
    }
}
