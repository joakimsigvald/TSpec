using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Return-value from an assertion, that allows another assertion to be chained to the previous (if it succeeded)
/// </summary>
/// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
public class ContinueWith<TContinuation> where TContinuation : Constraint
{
    private readonly TContinuation _continuation;
    internal ContinueWith(TContinuation continuation) => _continuation = continuation;

    /// <summary>
    /// Get a continuation to make the next assertion
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation and => Continue();

    /// <summary>
    /// Get a continuation to make the next assertion
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation but => Continue();

    /// <summary>
    /// Get a continuation to make the last alternative of an either
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation or => Continue();

    /// <summary>
    /// Get a continuation to make another alternative of an either, ahead of the last one after or
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TContinuation _ => Continue();

    private TContinuation Continue([CallerMemberName] string? conjunction = null)
    {
        var isEither = _continuation.State.HasFlag(ConstraintState.Either);
        var isAlternative = conjunction is "or" or "_";
        SpecificationContext.Current.AddAssertConjunction(conjunction!);
        if (isEither && !isAlternative)
            throw new SetupFailed(
                "Cannot continue either with and or but: separate its alternatives with _ and put or before the last");
        if (!isEither && isAlternative)
            throw new SetupFailed($"Cannot continue {conjunction} unless preceded with either");
        return conjunction switch
        {
            "or" => LastAlternative(),
            "_" => AnotherAlternative(),
            _ => NextAssertion(),
        };
    }

    private TContinuation LastAlternative()
    {
        SpecificationContext.Current.CloseEither();
        return Alternative(_continuation.State & ~ConstraintState.Either);
    }

    private TContinuation AnotherAlternative() => Alternative(_continuation.State);

    // An alternative keeps what the earlier ones decided, but a not before one of them is not its own.
    private TContinuation Alternative(ConstraintState state)
        => _continuation with { AuxiliaryVerb = null, State = state & ~ConstraintState.Inverted, WasInverted = false };

    // The next assertion stands on its own: the outcome and negation of the one before it do not carry over.
    private TContinuation NextAssertion()
        => _continuation with { AuxiliaryVerb = null, State = default, Failures = [], WasInverted = false };
}