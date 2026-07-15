using System.Diagnostics.CodeAnalysis;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Return-value from an assertion on the elements of a collection, that allows another assertion to be chained
/// to the previous, or applied to the matched element(s) through the property 'that'
/// </summary>
/// <typeparam name="TContinuation">The concrete type of the assertion continuation, enabling fluent chaining</typeparam>
/// <typeparam name="TThat">The type of the matched element(s), exposed through the property 'that'</typeparam>
public class ContinueWithThat<TContinuation, TThat> : ContinueWith<TContinuation> 
    where TContinuation : Constraint
{
    private readonly TThat _that;

    internal ContinueWithThat(TContinuation continuation, TThat that) : base(continuation)
        => _that = that;

    /// <summary>
    /// Continuation to apply assertions on the element
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TThat that
    {
        get
        {
            SpecificationContext.Current.AddThat();
            return _that;
        }
    }
}