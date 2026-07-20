using System.Diagnostics.CodeAnalysis;

namespace TSpec.Continuations;

/// <summary>
/// Return-value from a Throws assertion, that allows another assertion to be chained to the previous,
/// or applied to the thrown error through the property 'that'
/// </summary>
/// <typeparam name="TResult">The return type of the tested method</typeparam>
/// <typeparam name="TError">The type of the thrown error</typeparam>
public interface IThrowsThen<TResult, TError> : IAndThen<TResult>
{
    /// <summary>
    /// The thrown error, to apply further assertions on
    /// </summary>
    /// <example>
    /// <code>
    /// Then().Throws&lt;ArgumentException&gt;().that.Message.Is("Invalid cart");
    /// Then().Throws&lt;ArgumentException&gt;().that.Message.Does().Match(@"cart\d+");
    /// </code>
    /// </example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Special convention of binding words")]
    TError that { get; }
}
