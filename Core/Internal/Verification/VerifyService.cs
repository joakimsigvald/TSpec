using Moq;
using TSpec.Continuations;

namespace TSpec.Internal.Verification;

internal class VerifyService<TSUT, TResult, TService>(TestResult<TSUT, TResult> parent)
    : IVerifyService<TResult>
    where TService : class
{
    public IAndVerify<TResult> WasInvoked()
        => parent.VerifyInvoked<TService>(Moq.Times.AtLeastOnce(), null);

    public IAndVerify<TResult> WasInvoked(Moq.Times times, string? timesExpr)
        => parent.VerifyInvoked<TService>(times, timesExpr);

    public IAndVerify<TResult> WasInvoked(Func<Moq.Times> times, string? timesExpr)
        => parent.VerifyInvoked<TService>(times(), timesExpr);
}