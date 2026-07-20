using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations.Enumerable;

namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Object that allows assertions to be made on the characteristics of the provided string
/// </summary>
public record HasString : HasEnumerable<char>
{
    /// <summary>
    /// Asserts that the string has the given length
    /// </summary>
    /// <param name="expected">The expected length</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<HasStringContinuation> Length(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
    {
        var expectedStr = Express(expectedExpr, expected);
        Assert(expectedStr, NotNullAnd(actual => Xunit.Assert.Equal(expected, actual.Count())), expectedStr, "have");
        return new(ContinueString());
    }

    /// <summary>
    /// Continuations to make length-related assertions on the string
    /// </summary>
    /// <returns>A continuation for making further assertions on the string</returns>
    /// <example>
    /// <code>
    /// name.Has().Length(3);           // exactly three characters
    /// name.Has().Length().AtLeast(3); // length-related assertions such as AtLeast, AtMost and InRange
    /// </code>
    /// </example>
    public LengthContinuation Length()
        => new(this)
        {
            Actual = Actual,
            ActualExpr = ActualExpr,
            AuxiliaryVerb = AuxiliaryVerb,
            State = State,
        };

    internal HasStringContinuation ContinueString()
        => new()
        {
            Actual = Actual,
            ActualExpr = ActualExpr,
        };
}
