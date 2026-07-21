using System.Runtime.CompilerServices;
using Moq;

namespace TSpec.Continuations;

/// <summary>
/// A continuation to assert on the aggregate invocations of a mocked service.
/// </summary>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface IVerifyService<TResult>
{
    /// <summary>
    /// Assert that the service was invoked at least once (any method, property get/set or indexer).
    /// </summary>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> WasInvoked();

    /// <summary>
    /// Assert that the service was invoked (any method, property get/set or indexer) the given number of times.
    /// </summary>
    /// <param name="times">The number of times the service is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    /// <example>
    /// With <c>using static Moq.Times;</c> the method-group form reads naturally:
    /// <code>
    /// Then&lt;IEmailSender&gt;().WasInvoked(Never);
    /// Then&lt;IOrderService&gt;().WasInvoked(Once);
    /// </code>
    /// </example>
    IAndVerify<TResult> WasInvoked(
        Times times, [CallerArgumentExpression(nameof(times))] string? timesExpr = null);

    /// <summary>
    /// Assert that the service was invoked (any method, property get/set or indexer) the number of times given by a function.
    /// Enables the paren-free method-group form <c>WasInvoked(Once)</c> with <c>using static Moq.Times;</c>.
    /// </summary>
    /// <param name="times">A function providing the number of times the service is expected to have been invoked</param>
    /// <param name="timesExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation to apply additional assertions on the test result</returns>
    IAndVerify<TResult> WasInvoked(
        Func<Times> times, [CallerArgumentExpression(nameof(times))] string? timesExpr = null);
}
