using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TSpec.Assert.Continuations;

/// <summary>
/// What a failed assertion was about, expected and found, kept beside its failure so an assertion on each item
/// can word the failures of its items. Beside it rather than in a type of failure of its own: a spec catches
/// exactly <c>XunitException</c>.
/// </summary>
internal sealed record FailedExpectation(string Actual, string Expectation, string Outcome)
{
    private static readonly ConditionalWeakTable<Exception, FailedExpectation> _byFailure = [];

    internal void Attach(Exception failure) => _byFailure.AddOrUpdate(failure, this);

    /// <summary>
    /// Keeps what a failure expected beside the failure that reports it further up
    /// </summary>
    internal static void Carry(Exception from, Exception to)
    {
        if (TryRead(from, out var expectation))
            expectation.Attach(to);
    }

    internal static bool TryRead(Exception failure, [NotNullWhen(true)] out FailedExpectation? expectation)
        => _byFailure.TryGetValue(failure, out expectation);
}
