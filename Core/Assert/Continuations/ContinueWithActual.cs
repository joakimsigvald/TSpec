using System.Diagnostics.CodeAnalysis;

namespace TSpec.Assert.Continuations;

/// <summary>
/// Return-value from an assertion, that allows another assertion to be chained on the asserted value
/// through the properties 'and' and 'but'
/// </summary>
/// <typeparam name="TActual">The type of the value to assert on</typeparam>
/// <param name="actual">The value to assert on</param>
public class ContinueWithActual<TActual>(TActual? actual)
{
    /// <summary>
    /// Continuation to apply additional assertions on the value
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TActual and => actual!;

    /// <summary>
    /// Continuation to apply additional assertions on the value
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    public TActual but => actual!;
}