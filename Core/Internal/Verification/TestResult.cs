using Moq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;
using TSpec.Internal.TestData;
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
    /// Assert that the method under test threw a specific type of exception
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public IAndThen<TResult> Throws<TError>()
    {
        SpecificationContext.Current.AddAssertThrows<TError>();
        AssertError<TError>();
        return And();
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
        var conditionSpec = conditionExpr!.ParseValue();
        SpecificationContext.Current.AddAssertThrows<TError>($"where {conditionSpec}");
        AssertError(condition, conditionSpec);
        return And();
    }

    /// <summary>
    /// Assert that the method under test threw an exception
    /// </summary>
    /// <returns></returns>
    public IAndThen<TResult> Throws()
    {
        SpecificationContext.Current.AddAssert();
        AssertError<Exception>();
        return And();
    }

    /// <summary>
    /// Assert that the method under test did not throw a specific exception
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public IAndThen<TResult> DoesNotThrow<TError>()
    {
        SpecificationContext.Current.AddAssertDoesNotThrow<TError>();
        AssertNoError<TError>();
        return And();
    }

    /// <summary>
    /// Assert that the method under test did not throw any exception
    /// </summary>
    /// <returns></returns>
    public IAndThen<TResult> DoesNotThrow()
    {
        SpecificationContext.Current.AddAssert();
        AssertNoError<Exception>();
        return And();
    }

    internal IAndVerify<TResult> Verify<TService>(
        Expression<Action<TService>> expression, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression), expressionExpr);

    internal IAndVerify<TResult> Verify<TService>(
        Expression<Action<TService>> expression, Times times, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression, times), expressionExpr);

    internal IAndVerify<TResult> Verify<TService>(
        Expression<Action<TService>> expression, Func<Times> times, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression, times), expressionExpr);

    internal IAndVerify<TResult> Verify<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression), expressionExpr);

    internal IAndVerify<TResult> Verify<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Times times, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression, times), expressionExpr);

    internal IAndVerify<TResult> Verify<TService, TReturns>(
        Expression<Func<TService, TReturns>> expression, Func<Times> times, string expressionExpr)
        where TService : class
        => CombineWithErrorOnFail<TService>(mock => mock.Verify(expression, times), expressionExpr);

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

    private Mock<TObject> Mocked<TObject>() where TObject : class => _context.GetMock<TObject>();

    private AndVerify<TSUT, TResult> CombineWithErrorOnFail<TService>(Action<Mock<TService>> verify, string expressionExpr)
        where TService : class
    {
        try
        {
            SpecificationContext.Current.SetSubject(null);
            SpecificationContext.Current.AddVerify<TService>(expressionExpr);
            verify(Mocked<TService>());
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