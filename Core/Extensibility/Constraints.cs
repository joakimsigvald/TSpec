using TSpec.Assert.Continuations;

namespace TSpec.Extensibility;

/// <summary>
/// Constraints of an extension package, started as TSpec starts its own
/// </summary>
public static class Constraints
{
    /// <summary>
    /// The constraint an extension package's <c>Is()</c> returns, for its assertions on the value
    /// </summary>
    /// <typeparam name="TActual">The type of the value asserted on</typeparam>
    /// <typeparam name="TContinuation">The constraint</typeparam>
    /// <param name="actual">The value asserted on</param>
    /// <param name="actualExpr">How the value reads in the specification: the caller's
    /// <c>[CallerArgumentExpression]</c></param>
    /// <returns>The constraint, its assertions not yet made</returns>
    public static TContinuation Create<TActual, TContinuation>(TActual? actual, string actualExpr)
        where TContinuation : Constraint<TActual, TContinuation>, new()
        => Constraint<TActual, TContinuation>.Create(actual, actualExpr);
}
