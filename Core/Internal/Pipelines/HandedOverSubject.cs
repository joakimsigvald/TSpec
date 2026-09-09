using System.Runtime.CompilerServices;

namespace TSpec.Internal.Pipelines;

/// <summary>
/// The guard on a subject handed to Then or And. C# infers the subject's type from whatever is
/// passed, so a lambda or method group binds as happily as a value does and hands over a delegate.
/// </summary>
internal static class HandedOverSubject
{
    /// <summary>
    /// Running the pipeline does not affect a lambda, and a delegate exposes nothing a specification
    /// would claim, so what the author meant is the value calling it produces.
    /// </summary>
    internal static void AssertIsNotALambda<TSubject>(
        TSubject subject, string? subjectExpr, [CallerMemberName] string? verb = null)
    {
        if (subject is Delegate)
            throw new SetupFailed(
                $"{verb}({subjectExpr}) hands over a lambda, which running the pipeline does not affect "
                + "and which has nothing to assert on. Hand over the value it would produce instead");
    }
}
