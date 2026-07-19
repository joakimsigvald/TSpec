using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec;

public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test, without subject under test or return value</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Action act,
        [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test on the subject under test, without return value</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Action<TSUT> act,
        [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test on the subject under test and returning the result</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
    Func<TSUT, TResult?> act,
        [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the method-under-test, without subject under test, returning the result</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
    Func<TResult?> act, [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test on the subject under test, without return value</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Func<TSUT, Task> act, [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test, without subject under test or return value</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Func<Task> act, [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test on the subject under test and returning the result</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Func<TSUT, Task<TResult>> act, [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide the async method-under-test to the test-pipeline
    /// </summary>
    /// <param name="act">A lambda expression invoking the async method-under-test, without subject under test, returning the result</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> When(
        Func<Task<TResult>> act, [CallerArgumentExpression(nameof(act))] string? expr = null)
        => SetAction(act, expr!);

    /// <summary>
    /// Provide a teardown to the test-pipeline. Teardowns run in declaration order, after the method-under-test.
    /// </summary>
    /// <param name="tearDown">A lambda expression performing the teardown, given the subject under test</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> Until(
        Action<TSUT> tearDown, [CallerArgumentExpression(nameof(tearDown))] string? expr = null)
        => SetTearDown(tearDown, expr!);

    /// <summary>
    /// Provide an async teardown to the test-pipeline. Teardowns run in declaration order, after the method-under-test.
    /// </summary>
    /// <param name="tearDown">A lambda expression performing the async teardown, given the subject under test</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> Until(
        Func<TSUT, Task> tearDown, [CallerArgumentExpression(nameof(tearDown))] string? expr = null)
        => SetTearDown(tearDown, expr!);

    /// <summary>
    /// Provide a setup to the test-pipeline. Setups run in reverse declaration order, before the method-under-test.
    /// </summary>
    /// <param name="setUp">A lambda expression performing the setup, given the subject under test</param>
    /// <param name="delayMs">Delay between this method invocation and the next in the pipeline</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <param name="delayExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> Having(
        Action<TSUT> setUp,
        Func<int>? delayMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? expr = null,
        [CallerArgumentExpression(nameof(delayMs))] string? delayExpr = null)
    {
        AddDelay(delayMs, delayExpr!);
        return PrependSetUp(setUp, expr!);
    }

    /// <summary>
    /// Provide an async setup to the test-pipeline. Setups run in reverse declaration order, before the method-under-test.
    /// </summary>
    /// <param name="setUp">A lambda expression performing the async setup, given the subject under test</param>
    /// <param name="delayMs">Delay between this method invocation and the next in the pipeline</param>
    /// <param name="expr">Captured automatically by the compiler — do not provide</param>
    /// <param name="delayExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for providing further arrangement, or executing the test</returns>
    public ITestPipeline<TSUT, TResult> Having(
        Func<TSUT, Task> setUp, Func<int>? delayMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? expr = null,
        [CallerArgumentExpression(nameof(delayMs))] string? delayExpr = null)
    {
        AddDelay(delayMs, delayExpr!);
        return PrependSetUp(setUp, expr!);
    }

    private void AddDelay(Func<int>? delayMs, string delayExpr)
    {
        if (delayMs is null)
            return;

        PrependSetUp((Func<TSUT, Task>)(_ => Task.Delay(delayMs())), $"wait {delayExpr} ms");
    }

    private Spec<TSUT, TResult> SetAction(Delegate act, string expr)
    {
        Pipeline.SetAction(act, expr);
        return this;
    }

    private Spec<TSUT, TResult> SetTearDown(Delegate act, string expr)
    {
        Pipeline.SetTearDown(act, expr);
        return this;
    }

    private Spec<TSUT, TResult> PrependSetUp(Delegate act, string expr)
    {
        Pipeline.PrependSetUp(act, expr);
        return this;
    }
}