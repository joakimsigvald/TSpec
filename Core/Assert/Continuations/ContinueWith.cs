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

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    internal TContinuation or => Continue();

    private TContinuation Continue([CallerMemberName] string? conjunction = null) 
    {
        var isEither = _continuation.State.HasFlag(ConstraintState.Either);
        SpecificationContext.Current.AddAssertConjunction(conjunction!);
        if (isEither && conjunction != "or")
            throw new SetupFailed("Cannot continue either with and or but, only with or");
        if (!isEither && conjunction == "or")
            throw new SetupFailed("Cannot continue or unless preceded with either");
        return _continuation with 
        { 
            ActualExpr = string.Empty, 
            AuxiliaryVerb = null, 
            State = _continuation.State,
            Exception = _continuation.Exception
        };
    }
}