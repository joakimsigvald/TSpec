using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// The test pipeline, used to further arrange, specify and execute the test
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Runs the test pipeline and generates the result, which can be accessed by the Result property. When the test is run, any provided arrangement will be applied, then the subject-under-test will be created and the method-under-test called.
    /// </summary>
    /// <param name="_">Ignore this parameter — it exists only to distinguish overloads</param>
    /// <param name="because">An optional rationale justifying the expected outcome, included in the generated specification after the assertion.
    /// Phrase it to read naturally after the word "because". It can only be provided once per test method and covers all assertions chained after it</param>
    /// <returns>The test result, containing any return values or exceptions thrown, upon which assertions can be made</returns>
    ITestResultWithSUT<TSUT, TResult> Then(Ignore _ = default, string? because = null);

    /// <summary>
    /// Runs the test pipeline and generates the result, which can be accessed by the Result property. When the test is run, any provided arrangement will be applied, then the subject-under-test will be created and the method-under-test called.
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject to return</typeparam>
    /// <param name="subject">The subject to return for chained assertions</param>
    /// <param name="subjectExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The provided argument is returned, allowing assertions on the provided arguments to be chained</returns>
    TSubject Then<TSubject>(TSubject subject,
        [CallerArgumentExpression(nameof(subject))] string? subjectExpr = null);

    /// <summary>
    /// Run the test-pipeline, generate the result and verify that the given mock invocation was made.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    /// <example>
    /// Verify that the subject-under-test logged the message:
    /// <code>
    /// Then&lt;ILogger&gt;(_ =&gt; _.Log(The&lt;string&gt;()))
    /// </code>
    /// </example>
    IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class;

    /// <summary>
    /// Run the test-pipeline and continue with an aggregate invocation assertion on the given mocked service.
    /// </summary>
    /// <remarks>Deprecated: use <c>Then&lt;TService&gt;(wasInvoked: Times)</c> instead.</remarks>
    /// <typeparam name="TService">The mocked type to assert invocations on</typeparam>
    /// <returns>A continuation to assert on the aggregate invocations of the service</returns>
    [Obsolete("Use Then<TService>(wasInvoked: Times) instead, e.g. Then<IEmailSender>(wasInvoked: Never).")]
    IVerifyService<TResult> Then<TService>() where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify how many times the mocked service was invoked in aggregate — any method,
    /// property get/set or indexer access.
    /// </summary>
    /// <typeparam name="TService">The mocked type to assert invocations on</typeparam>
    /// <param name="_">Ignore this parameter — it exists only to force the wasInvoked argument to be named</param>
    /// <param name="wasInvoked">The number of times the service is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(Ignore _ = default, Times? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify how many times the mocked service was invoked in aggregate — any method,
    /// property get/set or indexer access.
    /// </summary>
    /// <typeparam name="TService">The mocked type to assert invocations on</typeparam>
    /// <param name="_">Ignore this parameter — it exists only to force the wasInvoked argument to be named</param>
    /// <param name="wasInvoked">A function providing the number of times the service is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(Ignore _ = default, Func<Times>? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. nameof(IEventQueue.MarkFailed)</param>
    /// <param name="wasInvoked">The number of times the method is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify how many times a named method of the mocked service was invoked, ignoring arguments.
    /// Matches any invocation of the named method regardless of arguments — ideal for asserting a method was not called.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="method">The name of the method to count invocations of, e.g. nameof(IEventQueue.MarkFailed)</param>
    /// <param name="wasInvoked">A function providing the number of times the method is expected to have been invoked</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(string method, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify that the given mock invocation was made the number of times given by a function.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made the given number of times.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">The number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class;

    /// <summary>
    /// Run the test-pipeline and verify that the given value-returning mock invocation was made the number of times given by a function.
    /// </summary>
    /// <typeparam name="TService">The mocked type to verify an invocation on</typeparam>
    /// <typeparam name="TReturns">The return type of the mocked invocation</typeparam>
    /// <param name="expression">An expression specifying the method invocation to verify</param>
    /// <param name="wasInvoked">A function providing the number of times the invocation is expected to have been made</param>
    /// <param name="expressionExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="wasInvokedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class;

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test on the subject under test, without return value</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Action<TSUT> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test, without subject under test or return value</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Action act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test on the subject under test and returning the result</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Func<TSUT, TResult?> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test, without subject under test, returning the result</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Func<TResult?> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test on the subject under test, without return value</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Func<TSUT, Task> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test, without subject under test or return value</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Func<Task> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test on the subject under test and returning the result</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(Func<TSUT, Task<TResult>> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test, without subject under test, returning the result</param>
    /// <param name="actExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    ITestPipeline<TSUT, TResult> When(
        Func<Task<TResult>> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null);

    /// <summary>
    /// Provide a Setup method that will be called before the method-under-test.
    /// Setup methods are executed in the opposite order that they are provided.
    /// </summary>
    /// <param name="setUp">the method to call as setup before executing the method-under-test</param>
    /// <param name="delayBeforeNextMs">Delay between this method invocation and the next in the pipeline</param>
    /// <param name="setUpExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="delayExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further arrangement to the test-pipeline</returns>
    ITestPipeline<TSUT, TResult> Having(
        Action<TSUT> setUp,
        Func<int>? delayBeforeNextMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? setUpExpr = null,
        [CallerArgumentExpression(nameof(delayBeforeNextMs))] string? delayExpr = null);

    /// <summary>
    /// Provide an async Setup method that will be called before the method-under-test.
    /// Setup methods are executed in the opposite order that they are provided.
    /// </summary>
    /// <param name="setUp">the async method to call as setup before executing the method-under-test</param>
    /// <param name="delayBeforeNextMs">Delay between this method invocation and the next in the pipeline</param>
    /// <param name="setUpExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="delayExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further arrangement to the test-pipeline</returns>
    ITestPipeline<TSUT, TResult> Having(
        Func<TSUT, Task> setUp,
        Func<int>? delayBeforeNextMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? setUpExpr = null,
        [CallerArgumentExpression(nameof(delayBeforeNextMs))] string? delayExpr = null);

    /// <summary>
    /// Provide a Teardown method that will be called on Dispose of the test class/fixture.
    /// Teardown methods are executed in the order that they are provided.
    /// </summary>
    /// <param name="tearDown">the method to call as teardown after executing the method-under-test</param>
    /// <param name="tearDownExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further arrangement to the test-pipeline</returns>
    ITestPipeline<TSUT, TResult> Until(Action<TSUT> tearDown,
        [CallerArgumentExpression(nameof(tearDown))] string? tearDownExpr = null);

    /// <summary>
    /// Provide an async Teardown method that will be called on Dispose of the test class/fixture.
    /// Teardown methods are executed in the order that they are provided.
    /// </summary>
    /// <param name="tearDown">the async method to call as teardown after executing the method-under-test</param>
    /// <param name="tearDownExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further arrangement to the test-pipeline</returns>
    ITestPipeline<TSUT, TResult> Until(Func<TSUT, Task> tearDown,
        [CallerArgumentExpression(nameof(tearDown))] string? tearDownExpr = null);

    /// <summary>
    /// Transform any value and use the transformed value as default
    /// </summary>
    /// <typeparam name="TValue">The type of the value to arrange</typeparam>
    /// <param name="transform">A function transforming the default value of the given type</param>
    /// <param name="transformExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null);

    /// <summary>
    /// Provide any arrangement to the test, which will be applied during test execution in reverse order of where in the test-pipeline it was provided
    /// </summary>
    /// <typeparam name="TValue">The type of the value to arrange</typeparam>
    /// <param name="setup">An action applied to each generated value of the given type</param>
    /// <param name="setupExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement of the test pipeline</returns>
    IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class;

    /// <summary>
    /// A continuation to provide further arrangement
    /// </summary>
    /// <returns>A continuation for providing test data and other arrangement</returns>
    IGivenContinuation<TSUT, TResult> Given();

    /// <summary>
    /// Access the mock of the given type to provide mock-setup
    /// </summary>
    /// <typeparam name="TService">The type to mock</typeparam>
    /// <returns>A continuation for providing mock-setup for the given type</returns>
    IGivenServiceContinuation<TSUT, TResult, TService> Given<TService>() where TService : class;

    /// <summary>
    /// Provide a tag to setup some expectation, such as associating it with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of value the tag is associated with</typeparam>
    /// <param name="tag">The tag</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for setting up an expectation on the tag</returns>
    IGivenTag<TSUT, TResult, TValue> Given<TValue>(
        Tag<TValue> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null);

    /// <summary>
    /// Provide a default value as a lambda, to be evaluated during test execution AFTER any subsequently added arrangement.
    /// Providing a default value as a lambda, to defer execution, is useful when the default value is created based on test data that is specified later in the test-pipeline.
    /// </summary>
    /// <typeparam name="TValue">The type of the default value</typeparam>
    /// <param name="defaultValue">A function providing the default value for the given type</param>
    /// <param name="scope">Determines whether the value is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the created value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="defaultValueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        Func<TValue> defaultValue,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(defaultValue))] string? defaultValueExpr = null);

    /// <summary>
    /// Provide a default value, that will be applied in all mocks and auto-generated test-data, where no specific value or setup is given.
    /// </summary>
    /// <typeparam name="TValue">The type of the default value</typeparam>
    /// <param name="defaultValue">The default value for the given type</param>
    /// <param name="scope">Determines whether the value is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="defaultValueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        TValue defaultValue,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(defaultValue))] string? defaultValueExpr = null);

    /// <summary>
    /// Instructs the test pipeline to use the value associated with the specified tag when resolving dependencies or generating test data.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the tag.</typeparam>
    /// <param name="tag">The tag used to identify the specific value instance.</param>
    /// <param name="scope">Determines whether the tag's value is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the tag's value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        Tag<TValue> tag,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null);
}