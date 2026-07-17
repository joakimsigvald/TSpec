using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Pipelines;

namespace TSpec;

public abstract partial class Spec<TSUT, TResult> : ITestPipeline<TSUT, TResult>
{
    /// <summary>
    /// Use values of the target type from the set described by another type, specified with From.
    /// If From is not called, the target type is instead registered as a concrete class to instantiate when the subject under test requires an abstraction it implements.
    /// </summary>
    /// <typeparam name="TTarget">The type being requested by the pipeline.</typeparam>
    /// <param name="scope">Determines whether the registration applies to Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <returns>A continuation to specify the source of the target type's values with From, or provide further arrangement.</returns>
    public IUsingContinuation<TSUT, TResult, TTarget> Using<TTarget>(For scope = For.All)
    {
        var continuation = new UsingContinuation<TSUT, TResult, TTarget>(this, scope);
        Pipeline.Specification.AddUsing(() => !continuation.IsConverted, typeof(TTarget).Name, scope);
        Pipeline.AppendUsing(continuation.ResolveDefault);
        return continuation;
    }

    /// <summary>
    /// Instructs the test pipeline to use the specified instance when resolving dependencies or generating test data.
    /// By default the instance remains owned by the caller: TSpec will not dispose it on teardown,
    /// unlike disposable objects TSpec creates itself for the subject-under-test graph.
    /// Pass owned: true to transfer ownership to the pipeline, which then disposes the instance after any Until-steps.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being provided.</typeparam>
    /// <param name="value">The specific instance to use.</param>
    /// <param name="scope">Determines whether the value is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="valueExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        TValue value,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
    {
        Pipeline.Using(value, scope, valueExpr!, owned);
        return new UsingTestPipeline<TSUT, TResult>(this);
    }

    /// <summary>
    /// Instructs the test pipeline to use a factory method to resolve the value when generating test data or resolving dependencies.
    /// By default factory-created instances remain owned by the caller: TSpec will not dispose them on teardown,
    /// unlike disposable objects TSpec creates itself for the subject-under-test graph.
    /// Pass owned: true to transfer ownership to the pipeline, which then disposes the created value after any Until-steps
    /// — useful e.g. for an HttpClient factory in integration tests.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being provided.</typeparam>
    /// <param name="factory">A function that creates the value.</param>
    /// <param name="scope">Determines whether the factory is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the created value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="factoryExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        Func<TValue> factory,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(factory))] string? factoryExpr = null)
    {
        Pipeline.AppendUsing(() => Pipeline.Using(factory, scope, factoryExpr!, owned));
        return new UsingTestPipeline<TSUT, TResult>(this);
    }

    /// <summary>
    /// Instructs the test pipeline to use the value associated with the specified tag when resolving dependencies or generating test data.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the tag.</typeparam>
    /// <param name="tag">The tag used to identify the specific value instance.</param>
    /// <param name="scope">Determines whether the tag's value is used for Subject Under Test construction (Subject), ambient test data (Input), or both (All). Defaults to All.</param>
    /// <param name="owned">When true, the pipeline takes ownership of the tag's value and disposes it on teardown, after any Until-steps. Defaults to false (caller-owned).</param>
    /// <param name="tagExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to provide further infrastructure and test data arrangement.</returns>
    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        Tag<TValue> tag,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
    {
        Pipeline.AppendUsing(() => Pipeline.Using(() => The(tag), scope, tagExpr!, owned));
        return new UsingTestPipeline<TSUT, TResult>(this);
    }
}