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
        => _parent.Using<TTarget>(scope);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        TValue value,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(value))] string? valueExpr = null)
        => _parent.Using(value, scope, owned, valueExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Func<TValue> factory,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(factory))] string? factoryExpr = null)
        => _parent.Using(factory, scope, owned, factoryExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Tag<TValue> tag,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => _parent.Using(tag, scope, owned, tagExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Action<TValue> setup,
        For scope = For.All,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null)
        where TValue : class
        => _parent.Using(setup, scope, setupExpr!);

    /// <inheritdoc />
    public IUsingTestPipeline<TSUT, TResult> And<TValue>(
        Func<TValue, TValue> transform,
        For scope = For.All,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => _parent.Using(transform, scope, transformExpr!);
}