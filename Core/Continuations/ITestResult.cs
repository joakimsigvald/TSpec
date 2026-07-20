using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// An object containing the result of a test-run
/// </summary>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface ITestResult<TResult>
{
    /// <summary>
    /// The return value of a non-throwing test-run
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Asserts that the test-run threw an error of the given type.
    /// The thrown error is exposed through 'that' for further assertions,
    /// e.g. Throws&lt;ArgumentException&gt;().that.Message.Is("Invalid cart")
    /// </summary>
    /// <typeparam name="TError">The type of the expected error</typeparam>
    /// <returns>A continuation to apply additional assertions on the thrown error or the test result</returns>
    IThrowsThen<TResult, TError> Throws<TError>();

    /// <summary>
    /// Asserts that the test-run threw the error instance provided by the given function (compared by reference).
    /// Pass a mention (e.g. The&lt;MyException&gt;) to verify that the error configured in the arrangement was propagated.
    /// To assert by type or content, use Throws&lt;TError&gt;() or Throws&lt;TError&gt;(condition).
    /// </summary>
    /// <typeparam name="TError">The type of the expected error</typeparam>
    /// <param name="expected">A function providing the expected error instance</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndThen<TResult> Throws<TError>(
        Func<TError> expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        where TError : Exception;

    /// <summary>
    /// Asserts that the test-run threw an error of the given type, and satisfies the given assertions
    /// </summary>
    /// <typeparam name="TError">The type of the expected error</typeparam>
    /// <param name="assert">An action applying assertions to the thrown error</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndThen<TResult> Throws<TError>(Action<TError> assert);

    /// <summary>
    /// Asserts that the test-run threw an error that satisfies the given predicate
    /// </summary>
    /// <typeparam name="TError">The type of the expected error</typeparam>
    /// <param name="condition">A predicate that the thrown error is expected to satisfy</param>
    /// <param name="conditionExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndThen<TResult> Throws<TError>(
        Func<TError, bool> condition,
        [CallerArgumentExpression(nameof(condition))] string? conditionExpr = null);

    /// <summary>
    /// Asserts that the test-run threw an error.
    /// The thrown error is exposed through 'that' for further assertions.
    /// </summary>
    /// <returns>A continuation to apply additional assertions on the thrown error or the test result</returns>
    IThrowsThen<TResult, Exception> Throws();

    /// <summary>
    /// Asserts that the test-run did not throw an error of the given type
    /// </summary>
    /// <typeparam name="TError">The type of error that is not expected</typeparam>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndThen<TResult> DoesNotThrow<TError>();

    /// <summary>
    /// Asserts that the test-run did not throw an error
    /// </summary>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndThen<TResult> DoesNotThrow();
}