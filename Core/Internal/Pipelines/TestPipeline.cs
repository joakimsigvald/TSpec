using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;

namespace TSpec.Internal.Pipelines;

internal abstract class TestPipeline<TSUT, TResult, TParent>(TParent parent) where TParent : Spec<TSUT, TResult>
{
    protected readonly TParent Parent = parent;

    public ITestPipeline<TSUT, TResult> When(
        Action<TSUT> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Action act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<TSUT, TResult?> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<TResult?> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<TSUT, Task> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<Task> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<TSUT, Task<TResult>> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> When(
        Func<Task<TResult>> act,
        [CallerArgumentExpression(nameof(act))] string? actExpr = null)
        => Parent.When(act, actExpr!);

    public ITestPipeline<TSUT, TResult> Having(
        Action<TSUT> setUp,
        Func<int>? delayBeforeNextMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? setUpExpr = null,
        [CallerArgumentExpression(nameof(delayBeforeNextMs))] string? delayExpr = null)
        => Parent.Having(setUp, delayBeforeNextMs, setUpExpr!, delayExpr!);

    public ITestPipeline<TSUT, TResult> Having(
        Func<TSUT, Task> setUp,
        Func<int>? delayBeforeNextMs = null,
        [CallerArgumentExpression(nameof(setUp))] string? setUpExpr = null,
        [CallerArgumentExpression(nameof(delayBeforeNextMs))] string? delayExpr = null)
        => Parent.Having(setUp, delayBeforeNextMs, setUpExpr!, delayExpr!);

    public ITestPipeline<TSUT, TResult> Until(
        Action<TSUT> tearDown, [CallerArgumentExpression(nameof(tearDown))] string? tearDownExpr = null)
        => Parent.Until(tearDown, tearDownExpr!);

    public ITestPipeline<TSUT, TResult> Until(
        Func<TSUT, Task> tearDown, [CallerArgumentExpression(nameof(tearDown))] string? tearDownExpr = null)
        => Parent.Until(tearDown, tearDownExpr!);

    public IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Action<TValue> setup,
        [CallerArgumentExpression(nameof(setup))] string? setupExpr = null) where TValue : class
        => Parent.Given(setup, setupExpr!);

    public IGivenTestPipeline<TSUT, TResult> Given<TValue>(
        Func<TValue, TValue> transform,
        [CallerArgumentExpression(nameof(transform))] string? transformExpr = null)
        => Parent.Given(transform, transformExpr!);

    public IGivenServiceContinuation<TSUT, TResult, TService> Given<TService>() where TService : class
        => Parent.Given<TService>();

    public IGivenContinuation<TSUT, TResult> Given() => Parent.Given();

    public IGivenTag<TSUT, TResult, TValue> Given<TValue>(
        Tag<TValue> tag,
        [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
        => Parent.Given(tag, tagExpr!);

    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        TValue defaultValue,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(defaultValue))] string? defaultValueExpr = null)
        => Parent.Using(defaultValue, scope, owned, defaultValueExpr!);

    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
        Func<TValue> defaultValue,
        For scope = For.All,
        bool owned = false,
        [CallerArgumentExpression(nameof(defaultValue))] string? defaultValueExpr = null)
        => Parent.Using(defaultValue, scope, owned, defaultValueExpr!);

    public IUsingTestPipeline<TSUT, TResult> Using<TValue>(
            Tag<TValue> tag,
            For scope = For.All,
            bool owned = false,
            [CallerArgumentExpression(nameof(tag))] string? tagExpr = null)
            => Parent.Using(tag, scope, owned, tagExpr!);

    public ITestResultWithSUT<TSUT, TResult> Then(Ignore _ = default, string? because = null) => Parent.Then(because: because);

    public TSubject Then<TSubject>(TSubject subject,
        [CallerArgumentExpression(nameof(subject))] string? subjectExpr = null)
        => Parent.Then(subject, subjectExpr);

    [Obsolete("Use Then<TService>(wasInvoked: Times) instead, e.g. Then<IEmailSender>(wasInvoked: Never).")]
    public IVerifyService<TResult> Then<TService>() where TService : class
        => Parent.Then<TService>();

    public IAndVerify<TResult> Then<TService>(Ignore _ = default, Times? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Parent.Then<TService>(_, wasInvoked, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService>(Ignore _ = default, Func<Times>? wasInvoked = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Parent.Then<TService>(_, wasInvoked, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService>(string method, Times wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Parent.Then<TService>(method, wasInvoked, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService>(string method, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null) where TService : class
        => Parent.Then<TService>(method, wasInvoked, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Parent.Then(expression, expressionExpr!);

    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Parent.Then(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService>(
        Expression<Action<TService>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Parent.Then(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null)
        where TService : class
        => Parent.Then(expression, expressionExpr!);

    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Parent.Then(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);

    public IAndVerify<TResult> Then<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Func<Times> wasInvoked,
        [CallerArgumentExpression(nameof(expression))] string? expressionExpr = null,
        [CallerArgumentExpression(nameof(wasInvoked))] string? wasInvokedExpr = null)
        where TService : class
        => Parent.Then(expression, wasInvoked, expressionExpr!, wasInvokedExpr!);
}