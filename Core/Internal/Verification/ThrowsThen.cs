using TSpec.Continuations;
using TSpec.Internal.Specification;

namespace TSpec.Internal.Verification;

internal class ThrowsThen<TSUT, TResult, TError> : AndThen<TSUT, TResult>, IThrowsThen<TResult, TError>
{
    private readonly TError _error;

    internal ThrowsThen(TestResult<TSUT, TResult> parent, TError error) : base(parent)
        => _error = error;

    public TError that
    {
        get
        {
            SpecificationContext.Current.AddThat();
            return _error;
        }
    }
}
