using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec;

public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Syntactic sugar for Then(because: reason). Run the test-pipeline, while providing a reason for the expected result, and return the result
    /// </summary>
    /// <param name="reason">A rationale justifying the expected outcome, included in the generated specification after the assertion.
    /// Phrase it to read naturally after the word "because". It can only be provided once per test method and covers all assertions chained after it</param>
    /// <returns>The test result</returns>
    public ITestResultWithSUT<TSUT, TResult> Because(string reason) => Then(because: reason);

    /// <summary>
    /// Run the test-pipeline and return the result
    /// </summary>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="because">An optional rationale justifying the expected outcome, included in the generated specification after the assertion.
    /// Phrase it to read naturally after the word "because". It can only be provided once per test method and covers all assertions chained after it</param>
    /// <returns>The test result</returns>
    public ITestResultWithSUT<TSUT, TResult> Then(Ignore _ = default, string? because = null) => Pipeline.Then(because);

    /// <summary>
    /// Run the test-pipeline and return a given subject to be used in chained assertions.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject to return</typeparam>
    /// <param name="subject">The subject to return for chained assertions</param>
    /// <param name="subjectExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>the given subject</returns>
    public TSubject Then<TSubject>(TSubject subject,
        [CallerArgumentExpression(nameof(subject))] string? subjectExpr = null)
        => Pipeline.Then(subject, subjectExpr!);

    /// <summary>
    /// Run the test-pipeline and continue with an aggregate invocation assertion on the given mocked service.
    /// </summary>
    /// <typeparam name="TService">The mocked type to assert invocations on</typeparam>
    /// <returns>A continuation to assert on the aggregate invocations of the service</returns>
    /// <example>
    /// Verify the subject did not touch a collaborator, or bounded its calls:
    /// <code>
    /// Then&lt;IEmailSender&gt;().WasInvoked(Never);   // using static Moq.Times;
    /// Then&lt;IOrderService&gt;().WasInvoked(Once);
    /// </code>
    /// </example>
    public IVerifyService<TResult> Then<TService>() where TService : class
        => Pipeline.Then<TService>();

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// </summary>
    /// <remarks>
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called
    /// (<c>Never</c>). Prefer <c>nameof</c> for a refactor-safe name. On an overloaded method the count aggregates across
    /// all overloads; use the expression form when a specific overload or argument values matter.
    /// </remarks>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. <c>nameof(IEventQueue.MarkFailed)</c></param>
    /// <param name="times">The number of times the method is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    /// <example>
    /// <code>
    /// Then&lt;IEventQueue&gt;(nameof(IEventQueue.MarkFailed), Never); // using static Moq.Times;
    /// </code>
    /// </example>
    public IAndVerify<TResult> Then<TService>(string method, Times times,
        [CallerArgumentExpression(nameof(times))] string? timesExpr = null) where TService : class
        => Pipeline.Then<TService>(method, times, timesExpr!);

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// </summary>
    /// <remarks>
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called
    /// (<c>Never</c>). Prefer <c>nameof</c> for a refactor-safe name. On an overloaded method the count aggregates across
    /// all overloads; use the expression form when a specific overload or argument values matter.
    /// </remarks>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. <c>nameof(IEventQueue.MarkFailed)</c></param>
    /// <param name="times">A function providing the number of times the method is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(string method, Func<Times> times,
        [CallerArgumentExpression(nameof(times))] string? timesExpr = null) where TService : class
        => Pipeline.Then<TService>(method, times, timesExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    /// <example>
    /// Verify that the subject-under-test logged the message:
    /// <code>
    /// Then&lt;ILogger&gt;(_ =&gt; _.Log(The&lt;string&gt;()))
    /// </code>
    /// </example>
    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Pipeline.Then(expression, expressionExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null) where TService : class
        => Pipeline.Then(expression, times, expressionExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made the number of times given by a function.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null) where TService : class
        => Pipeline.Then(expression, times, expressionExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null) where TService : class
        => Pipeline.Then(expression, expressionExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Pipeline.Then(expression, times, expressionExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made the number of times given by a function.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="times">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Func<Times> times,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Pipeline.Then(expression, times, expressionExpr!);

    /// <summary>
    /// Contains the returned value after calling method-under-test.
    /// Accessing this property runs the test pipeline if it has not been run yet.
    /// </summary>
    protected TResult Result => Pipeline.TestResult.Result;
}