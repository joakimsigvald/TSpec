using System.Runtime.CompilerServices;
using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Verification;

internal class AndThen<TSUT, TResult> : IAndThen<TResult>
{
    internal protected readonly TestResult<TSUT, TResult> _parent;
    internal AndThen(TestResult<TSUT, TResult> parent) => _parent = parent;

    /// <summary>
    /// Continuation to make additional assertions on the result
    /// </summary>
    /// <returns></returns>
    public ITestResult<TResult> and
    {
        get
        {
            SpecificationContext.Current.AddThen();
            return _parent;
        }
    }

    /// <summary>
    /// Continuation to make additional assertions on the subject
    /// </summary>
    /// <typeparam name="TSubject"></typeparam>
    /// <param name="subject"></param>
    /// <param name="subjectExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns></returns>
    public TSubject And<TSubject>(TSubject subject,
        [CallerArgumentExpression(nameof(subject))] string? subjectExpr = null)
    {
        subjectExpr.AssertNoTrainwreck();
        SpecificationContext.Current.AddThen();
        SpecificationContext.Current.SetSubject(subjectExpr);
        return subject;
    }
}