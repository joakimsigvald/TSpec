using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Mocking;

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
    /// Run the test-pipeline and verify how many times the mocked service was invoked in aggregate — any method,
    /// property get/set or indexer access.
    /// </summary>
    /// <typeparam name="TService">The mocked type to assert invocations on</typeparam>
    /// <param name="_">Ignore this parameter — it exists only to force the <c>wasInvoked</c> argument to be named</param>
    /// <param name="wasInvoked">The number of times the service is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    /// <example>
    /// Verify the subject did not touch a collaborator, or bounded its calls:
    /// <code>
    /// Then&lt;IEmailSender&gt;(wasInvoked: Never);   // using static TSpec.Times;
    /// Then&lt;IOrderService&gt;(wasInvoked: Once);
    /// </code>
    /// </example>
    public IAndVerify<TResult> Then<TService>(Ignore _ = default, Times? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.ThenWasInvoked(MockTarget<TService>.Family, RequireWasInvoked(wasInvoked), wasInvokedExpr!);

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
    /// <param name="wasInvoked">The number of times the method is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    /// <example>
    /// <code>
    /// Then&lt;IEventQueue&gt;(nameof(IEventQueue.MarkFailed), Never); // using static TSpec.Times;
    /// </code>
    /// </example>
    public IAndVerify<TResult> Then<TService>(string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.Then(MockTarget<TService>.Family, method, wasInvoked, wasInvokedExpr!);

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
        => Pipeline.Then(MockTarget<TService>.Family, expression, null, expressionExpr!, null);

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.Then(MockTarget<TService>.Family, expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

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
        => Pipeline.Then(MockTarget<TService>.Family, expression, null, expressionExpr!, null);

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Pipeline.Then(MockTarget<TService>.Family, expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify how many times the one mock a mention holds was invoked in aggregate.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The mention, as a method group: <c>Then(TheSecond&lt;IRule&gt;, wasInvoked: Never)</c></param>
    /// <param name="wasInvoked">The number of times the mock is expected to have been invoked</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(Func<TService> mock, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.ThenWasInvoked(MockTarget<TService>.Of(mock, mockExpr!), wasInvoked, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the one mock a mention holds was invoked, ignoring arguments.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The mention, as a method group: <c>Then(TheSecond&lt;IRule&gt;, nameof(IRule.Passes), Never)</c></param>
    /// <param name="method">The name of the method to count invocations of</param>
    /// <param name="wasInvoked">The number of times the method is expected to have been invoked</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(Func<TService> mock, string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), method, wasInvoked, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given invocation was made on the one mock a mention holds.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The mention, as a method group: <c>Then(TheSecond&lt;IRule&gt;, _ =&gt; _.Passes())</c></param>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Func<TService> mock,
        Expression<Action<TService>> expression,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), expression, null, expressionExpr!, null);

    /// <summary>
    /// Run the test-pipeline and verify that the given invocation was made on the one mock a mention holds the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The mention, as a method group: <c>Then(TheSecond&lt;IRule&gt;, _ =&gt; _.Passes(), Once)</c></param>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Func<TService> mock,
        Expression<Action<TService>> expression,
        Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify how many times the one mock a tag holds was invoked in aggregate.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The tag of the mock</param>
    /// <param name="wasInvoked">The number of times the mock is expected to have been invoked</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(Tag<TService> mock, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.ThenWasInvoked(MockTarget<TService>.Of(mock, mockExpr!), wasInvoked, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the one mock a tag holds was invoked, ignoring arguments.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The tag of the mock</param>
    /// <param name="method">The name of the method to count invocations of</param>
    /// <param name="wasInvoked">The number of times the method is expected to have been invoked</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(Tag<TService> mock, string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), method, wasInvoked, wasInvokedExpr!);

    /// <summary>
    /// Run the test-pipeline and verify that the given invocation was made on the one mock a tag holds.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The tag of the mock</param>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Tag<TService> mock,
        Expression<Action<TService>> expression,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), expression, null, expressionExpr!, null);

    /// <summary>
    /// Run the test-pipeline and verify that the given invocation was made on the one mock a tag holds the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type</typeparam>
    /// <param name="mock">The tag of the mock</param>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="mockExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for further verification or assertions of the test result</returns>
    public IAndVerify<TResult> Then<TService>(
        Tag<TService> mock,
        Expression<Action<TService>> expression,
        Times wasInvoked,
        [CallerArgumentExpression(nameof(mock))] string? mockExpr = null,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Pipeline.Then(MockTarget<TService>.Of(mock, mockExpr!), expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    /// <summary>
    /// Contains the returned value after calling method-under-test.
    /// Accessing this property runs the test pipeline if it has not been run yet.
    /// </summary>
    protected TResult Result => Pipeline.Claim.Result;

    private static Times RequireWasInvoked(Times? wasInvoked)
        => wasInvoked ?? throw MissingWasInvoked;


    private static SetupFailed MissingWasInvoked
        => new("Then<TService>() requires a 'wasInvoked' argument, e.g. Then<TService>(wasInvoked: Never)");
}