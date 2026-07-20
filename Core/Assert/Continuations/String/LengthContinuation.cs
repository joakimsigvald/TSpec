using System.Runtime.CompilerServices;
using TSpec.Assert.Continuations.Enumerable;
using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.String;

/// <summary>
/// Provides assertion methods for verifying the length of a string within a fluent assertion chain.
/// </summary>
/// <remarks>Use this class to assert that a string meets specific length-related conditions, such as having
/// at least, at most, or a range of characters. Methods return a continuation to allow
/// chaining further assertions on the string.</remarks>
public record LengthContinuation : EnumerableConstraint<char, HasEnumerableContinuation<char>>
{
    private readonly HasString _parent;

    internal LengthContinuation(HasString parent) => _parent = parent;

    /// <summary>
    /// Asserts that the string has at least the given length
    /// </summary>
    /// <param name="expected">Lowest allowed length</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<HasStringContinuation> AtLeast(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => AssertLength($"length at least {Express(expectedExpr, expected)}",
            actual => Xunit.Assert.True(actual.Count() >= expected));

    /// <summary>
    /// Asserts that the string has at most the given length
    /// </summary>
    /// <param name="expected">Highest allowed length</param>
    /// <param name="expectedExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<HasStringContinuation> AtMost(
        int expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpr = null)
        => AssertLength($"length at most {Express(expectedExpr, expected)}",
            actual => Xunit.Assert.True(actual.Count() <= expected));

    /// <summary>
    /// Asserts that the string has length between (including) from and to
    /// </summary>
    /// <param name="from">Lowest allowed length</param>
    /// <param name="to">Highest allowed length</param>
    /// <param name="fromExpr">Captured automatically by the compiler — do not provide</param>
    /// <param name="toExpr">Captured automatically by the compiler — do not provide</param>
    /// <returns>A continuation for making further assertions on the string</returns>
    public ContinueWith<HasStringContinuation> InRange(
        int from,
        int to,
        [CallerArgumentExpression(nameof(from))] string? fromExpr = null,
        [CallerArgumentExpression(nameof(to))] string? toExpr = null)
    {
        if (from > to)
            throw new SetupFailed("Given range must be in ascending order");

        return AssertLength($"length between {Express(fromExpr, from)} and {Express(toExpr, to)}",
            actual =>
            {
                var actualLength = actual.Count();
                Xunit.Assert.True(actualLength >= from && actualLength <= to);
            });
    }

    private ContinueWith<HasStringContinuation> AssertLength(string expectedStr, Action<IEnumerable<char>> assert)
    {
        Assert(expectedStr, NotNullAnd(assert), expectedStr, "have", methodName: null);
        return new(_parent.ContinueString());
    }

    /// The failure description states the actual length before the string itself
    private protected override string Describe(IEnumerable<char>? value, string? methodName = null)
        => value is null ? "null" : $"{value.Count()}: {value.FormatValue()}";

    internal override HasEnumerableContinuation<char> Continue() => _parent.Continue();
}
