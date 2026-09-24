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
public interface IGivenServiceContinuation<TSUT, TResult, TService> : IGivenMockContinuation<TSUT, TResult, TService>
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
    /// Mock a member by name, whatever its arguments. It is a default: a setup of a specific call wins
    /// over it. Prefer nameof, so the compiler checks the name.
    /// </summary>
    /// <typeparam name="TReturns">The type the member answers with; for an async member, the value inside the task</typeparam>
    /// <param name="member">The name of the member to mock, e.g. nameof(IRoomStore.Find)</param>
    /// <returns>A continuation for providing the result to mock</returns>
    /// <example>
    /// <code>
    /// Given&lt;HttpMessageHandler&gt;().That&lt;HttpResponseMessage&gt;("SendAsync").Returns(A&lt;HttpResponseMessage&gt;)
    /// </code>
    /// </example>
    IGivenThatContinuation<TSUT, TResult, TService, TReturns> That<TReturns>(string member);

    /// <summary>
    /// Mock a member answering with nothing — void, or a task carrying no value — by name, whatever its
    /// arguments. It is a default: a setup of a specific call wins over it. Prefer nameof, so the
    /// compiler checks the name.
    /// </summary>
    /// <param name="member">The name of the member to mock, e.g. nameof(IRoomStore.Save)</param>
    /// <returns>A continuation for providing the result to mock</returns>
    IGivenThatVoidContinuation<TSUT, TResult, TService> That(string member);

    /// <summary>
    /// Obsolete: That&lt;TReturns&gt;(member) reaches a protected member too.
    /// </summary>
    /// <typeparam name="TReturns">The type the member answers with</typeparam>
    /// <param name="member">The name of the member to mock</param>
    /// <returns>A continuation for providing the result to mock</returns>
    [Obsolete("Use That<TReturns>(member), which reaches a protected member too")]
    IGivenThatContinuation<TSUT, TResult, TService, TReturns> ThatProtected<TReturns>(string member);

    /// <summary>
    /// Obsolete: That(member) reaches a protected member too.
    /// </summary>
    /// <param name="member">The name of the member to mock</param>
    /// <returns>A continuation for providing the result to mock</returns>
    [Obsolete("Use That(member), which reaches a protected member too")]
    IGivenThatVoidContinuation<TSUT, TResult, TService> ThatProtected(string member);
}