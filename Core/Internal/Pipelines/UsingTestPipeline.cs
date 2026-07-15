using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// The internal implementation of the test pipeline continuation for infrastructure and data generation arrangement.
/// Delegates subsequent setup calls back to the parent specification.
/// </summary>
internal class UsingTestPipeline<TSUT, TResult> :
    TestPipeline<TSUT, TResult, Spec<TSUT, TResult>>,
    IUsingTestPipeline<TSUT, TResult>
{
    internal UsingTestPipeline(Spec<TSUT, TResult> parent) : base(parent)
    {
    }

    /// <inheritdoc />
    public IUsingContinuation<TSUT, TResult, TTarget> And<TTarget>(For scope = For.All)
        => Parent.Using<TTarget>(scope);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        TValue value,
        For scope = For.All,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => Parent.Using(value, scope, valueExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Func<TValue> factory,
        For scope = For.All,
        [CallerArgumentExpression(nameof(factory))] string? factoryExpr = null)
        => Parent.Using(factory, scope, factoryExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Tag<TValue> tag,
        For scope = For.All,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Parent.Using(tag, scope, tagExpr!);
}