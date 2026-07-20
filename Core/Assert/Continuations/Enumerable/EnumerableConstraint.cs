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

    private protected static string Express<TValue>(string? valueExpr, TValue value)
    {
        var valueStr = value.FormatValue();
        return valueExpr is null || valueExpr == valueStr ? valueStr : $"'{valueExpr.ParseValue()}' = {value}";
    }

    private protected static Action<IEnumerable<TItem>?> NotEmptyAnd(Action<IEnumerable<TItem>> assert)
        => actual =>
        {
            Xunit.Assert.NotNull(actual);
            Xunit.Assert.NotEmpty(actual);
            assert(actual);
        };
}