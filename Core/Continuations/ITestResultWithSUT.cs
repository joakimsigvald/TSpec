namespace TSpec.Continuations;

/// <summary>
/// The result of a test-run that, in addition to the outcome, exposes the subject under test for further assertions
/// </summary>
/// <typeparam name="TSUT">The type of the subject under test</typeparam>
/// <typeparam name="TResult">The return type of the method-under-test</typeparam>
public interface ITestResultWithSUT<TSUT, TResult> : ITestResult<TResult>
{
    /// <summary>
    /// Provide the subject under test for non-static test methods.
    /// For static test-methods only returns the default- or auto-generated value of the type declared as Subject under test.
    /// </summary>
    TSUT SubjectUnderTest { get; }
}