using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows assertions to be made on the provided enumerable
/// </summary>
public abstract record EnumerableConstraint<TItem, TContinuation> : Constraint<IEnumerable<TItem>, TContinuation>
    where TContinuation : EnumerableConstraint<TItem, TContinuation>, new()
{
    static readonly string[] _methodsWithCount = ["Single", "Count", "Length", "OneItem", "TwoItems", "ThreeItems", "FourItems", "FiveItems"];

    private protected override string Describe(IEnumerable<TItem>? value, string? methodName = null)
        => value is not null && _methodsWithCount.Contains(methodName)
            ? $"{value.Count()}: {value.FormatValue()}"
            : value.FormatValue();

    /// <summary>
    /// A named value says both what it is called and what it was: the name alone would not say
    /// which value failed, and the value alone would not say what the test called it. Where the
    /// name is a theory parameter the value is the row's rather than the requirement's, so it is
    /// marked as a hole — kept here, dropped by the document, which tables every row instead.
    /// </summary>
    private protected static string Express<TValue>(string? valueExpr, TValue value)
    {
        var valueStr = value.FormatValue();
        if (valueExpr is null || valueExpr == valueStr)
            return valueStr;
        var named = $"'{valueExpr.Describe()}'";
        var assigned = $" = {value!.InvariantText()}";
        return SpecificationContext.IsHole(valueExpr) ? named + Hole.Mark(assigned) : named + assigned;
    }

    private protected static Action<IEnumerable<TItem>?> NotEmptyAnd(Action<IEnumerable<TItem>> assert)
        => actual =>
        {
            Xunit.Assert.NotNull(actual);
            Xunit.Assert.NotEmpty(actual);
            assert(actual);
        };
}