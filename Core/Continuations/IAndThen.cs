using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TSpec.Continuations;

/// <summary>
/// A continuation object to apply additional assertions to a test-run
/// </summary>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface IAndThen<TResult>
{
    /// <summary>
    /// Provides the result of the test-run, to apply additional assertions
    /// </summary>
    /// <returns>The test result, upon which additional assertions can be made</returns>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    ITestResult<TResult> and { get; }

    /// <summary>
    /// Provides any subject to apply additional assertions on
    /// </summary>
    /// <typeparam name="TSubject">The type of the subject to return</typeparam>
    /// <param name="subject">The subject to return for chained assertions</param>
    /// <param name="subjectExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>The provided subject</returns>
    TSubject And<TSubject>(TSubject subject,
        [CallerArgumentExpression(nameof(subject))] string? subjectExpr = null);
}