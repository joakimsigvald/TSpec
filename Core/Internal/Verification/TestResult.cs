using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
using TSpec.Internal.TestData.Generation.Strategies.Mocking;
using Xunit.Sdk;

namespace TSpec.Internal.Verification;

internal class TestResult<TSUT, TResult> : ITestResultWithSUT<TSUT, TResult>
{
    private readonly Exception? _error;
    private readonly Context _context;
    private readonly bool _hasResult;
    private TResult? _result;

    internal TestResult(
        TSUT tsut,
        TResult result,
        Exception? error,
        Context context,
        bool hasResult)
    {
        SubjectUnderTest = tsut;
        Result = result;
        _error = error;
        _context = context;
        _hasResult = hasResult;
    }

    /// <summary>
    /// Provide the return value of the tested method
    /// </summary>
    public TResult Result
    {
        get => _hasResult && _error is null ? _result! : throw UnexpectedError;
        init => _result = value;
    }

    /// <summary>
    /// Provide the subject under test for non-static test methods.
    /// For static test-methods only returns the default- or auto-generated value of the type declared as Subject under test.
    /// </summary>
    public TSUT SubjectUnderTest { get; }

    /// <summary>
    /// Assert that the method under test threw a specific type of exception.
    /// The thrown exception is exposed through 'that' for further assertions.
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public IThrowsThen<TResult, TError> Throws<TError>()
    {
        SpecificationContext.Current.AddAssertThrows<TError>();
        var error = AssertError<TError>();
        return new ThrowsThen<TSUT, TResult, TError>(this, error);
    }

    /// <summary>
    /// Assert that the method under test threw a specific exception instance (compared by reference).
    /// Pass a mention (e.g. The&lt;MyException&gt;) to verify that the exception configured in the arrangement
    /// was propagated. To assert by type or content, use Throws&lt;TError&gt;() or Throws&lt;TError&gt;(condition).
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="expected"></param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndThen<TResult> Throws<TError>(
        Func<TError> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        where TError : Exception
    {
        SpecificationContext.Current.AddAssertThrows(expectedExpr!);
        AssertError(expected());
        return And();
    }

    /// <summary>
    /// Assert that the method under test threw a specific type of exception, satisfying some condition (assert)
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="assert"></param>
    /// <returns></returns>
    public IAndThen<TResult> Throws<TError>(
        Action<TError> assert)
    {
        SpecificationContext.Current.AddAssertThrows<TError>("where");
        AssertError(assert);
        return And();
    }

    /// <summary>
    /// Assert that the method under test threw a specific type of exception, satisfying some condition
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="condition"></param>
    /// <param name="conditionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public IAndThen<TResult> Throws<TError>(
        Func<TError, bool> condition, [CallerArgumentExpression(nameof(condition))] string? conditionExpr = null)
    {
        var conditionSpec = conditionExpr!.Describe();
        SpecificationContext.Current.AddAssertThrows<TError>($"where {conditionSpec}");
        AssertError(condition, conditionSpec);
        return And();
    }

    /// <summary>
    /// Assert that the method under test threw an exception.
    /// The thrown exception is exposed through 'that' for further assertions.
    /// </summary>
    /// <returns></returns>
    public IThrowsThen<TResult, Exception> Throws()
    {
        SpecificationContext.Current.AddAssert();
        var error = AssertError<Exception>();
        return new ThrowsThen<TSUT, TResult, Exception>(this, error);
    }

    /// <summary>
    /// Assert that the method under test ran to the end — it returned rather than threw.
    /// </summary>
    /// <returns></returns>
    public IAndThen<TResult> Completes()
    {
        SpecificationContext.Current.AddAssert();
        AssertNoError<Exception>();
        return And();
    }

    internal IAndVerify<TResult> VerifyInvoked<TService>(Times times, string? timesExpr)
        where TService : class
    {
        var expectation = DescribeInvocationTimes(timesExpr);
        try
        {
            SpecificationContext.Current.ClearSubject();
            SpecificationContext.Current.AddWasInvoked<TService>(timesExpr);
            var mock = _context.GetMock<TService>();
            var count = mock.CountedInvocations.Count;
            if (times.Allows(count))
                return new AndVerify<TSUT, TResult>(this);

            var failure = CountNotMet(typeof(TService).Alias(), expectation, count);
            // A count of every call the mock received already says, at 0, that there were none
            throw count == 0 ? new XunitException(failure) : WithReceivedCalls(failure, mock);
        }
        catch (Exception ex)
        {
            if (_error is null)
                throw;
            throw new AggregateException(ex, _error);
        }
    }

    internal IAndVerify<TResult> VerifyInvoked<TService>(string method, Times times, string? timesExpr)
        where TService : class
    {
        var expectation = DescribeInvocationTimes(timesExpr);
        try
        {
            VerificationByName.AssertNamesAMethod(typeof(TService), method);
            SpecificationContext.Current.ClearSubject();
            SpecificationContext.Current.AddWasInvoked<TService>(method, timesExpr);
            var mock = _context.GetMock<TService>();
            var count = mock.CountedInvocations.Count(i => i.Method.Name == method);
            if (!times.Allows(count))
                throw WithReceivedCalls(CountNotMet($"{typeof(TService).Alias()}.{method}", expectation, count), mock);
            return new AndVerify<TSUT, TResult>(this);
        }
        catch (Exception ex)
        {
            if (_error is null)
                throw;
            throw new AggregateException(ex, _error);
        }
    }

    internal static string DescribeInvocationTimes(string? timesExpr)
        => timesExpr.NormalizeTimes() switch
        {
            "" or "AtLeastOnce" => "at least once",
            "Never" => "never",
            "Once" => "once",
            var normalized => normalized,
        };

    private static string CountNotMet(string call, string expectation, int count)
        => $"Expected {call} to be invoked {expectation} but {DescribeCount(count)}";

    private static XunitException WithReceivedCalls(string failure, MockHandle mock)
        => new($"{failure}{Environment.NewLine}{ReceivedCalls.Of(mock)}");

    private static string DescribeCount(int count)
        => count switch
        {
            0 => "was never invoked",
            1 => "was invoked once",
            _ => $"was invoked {count} times"
        };

    internal IAndVerify<TResult> Verify<TService>(
        Expression<Action<TService>> expression, string expressionExpr)
        where TService : class
        => VerifyCall<TService>(expression, null, expressionExpr, null);

    internal IAndVerify<TResult> Verify<TService>(
        Expression<Action<TService>> expression, Times wasInvoked, string expressionExpr, string? wasInvokedExpr)
        where TService : class
        => VerifyCall<TService>(expression, wasInvoked, expressionExpr, wasInvokedExpr);

    internal IAndVerify<TResult> Verify<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, string expressionExpr)
        where TService : class
        => VerifyCall<TService>(expression, null, expressionExpr, null);

    internal IAndVerify<TResult> Verify<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times wasInvoked, string expressionExpr, string? wasInvokedExpr)
        where TService : class
        => VerifyCall<TService>(expression, wasInvoked, expressionExpr, wasInvokedExpr);

    private void AssertError<TError>(TError expected)
        where TError : Exception
    {
        var actual = AssertError<TError>();
        if (ReferenceEquals(expected, actual))
            return;

        throw new XunitException(IsLookalike()
            ? "Expected the exact exception instance, but a different instance with the same type and message was thrown"
            : $"Expected the exception {expected}, but {actual} was thrown");

        bool IsLookalike() => expected.GetType() == actual.GetType() && expected.Message == actual.Message;
    }

    private void AssertError<TError>(Action<TError> assert)
    {
        try
        {
            assert(AssertError<TError>());
        }
        catch (Exception ex)
        {
            throw new XunitException($"Thrown exception {typeof(TError)} didn't meet expectations", ex);
        }
    }

    private void AssertError<TError>(Func<TError, bool> predicate, string predicateExpr)
    {
        var error = AssertError<TError>();
        if (!predicate(error))
            throw new XunitException($"Thrown exception {typeof(TError)} didn't satisfy {predicateExpr}.");
    }

    private TError AssertError<TError>()
        => _error is TError err
        ? err
        : throw new XunitException(
            $"Expected {typeof(TError)}, but {_error?.GetType().Name ?? "No exception"} was thrown");

    private void AssertNoError<TError>()
    {
        if (_error is TError)
            throw new XunitException($"Expected not to throw {typeof(TError)}, but threw {_error.GetType().Name}");
    }

    private Exception UnexpectedError
        => _error ?? new SetupFailed(
@"Tried to use Result, but an action, or func with different return type, was provided as method under test (When). 
Try providing a function with the Spec's declared return type instead as parameter to When");

    /// A call named by an expression is counted among the calls the mock received, and fails the way
    /// a count by name does — at least once unless a count is given.
    private AndVerify<TSUT, TResult> VerifyCall<TService>(
        LambdaExpression call, Times? times, string callExpr, string? timesExpr)
        where TService : class
    {
        try
        {
            SpecificationContext.Current.ClearSubject();
            SpecificationContext.Current.AddVerify<TService>(callExpr, timesExpr);
            var mock = _context.GetMock<TService>();
            var count = mock.CountCalls(call, callExpr);
            if (!(times ?? Times.AtLeastOnce).Allows(count))
                throw WithReceivedCalls(
                    CountNotMet(
                        callExpr.DescribeMockCallOn<TService>().StripWrapMarkers(),
                        DescribeInvocationTimes(timesExpr),
                        count),
                    mock);
            return new AndVerify<TSUT, TResult>(this);
        }
        catch (Exception ex)
        {
            if (_error is null)
                throw;
            throw new AggregateException(ex, _error);
        }
    }

    private AndThen<TSUT, TResult> And() => new(this);
}