using TSpec.Internal.Specification;

namespace TSpec.Assert.Continuations.Enumerable;

/// <summary>
/// Object that allows assertions to be made on the provided enumerable
/// </summary>
public abstract record EnumerableConstraint<TItem, TContinuation> : Constraint<IEnumerable<TItem>, TContinuation>
    where TContinuation : EnumerableConstraint<TItem, TContinuation>, new()
{
    static readonly string[] _methodsWithCount = ["Single", "Count", "OneItem", "TwoItems", "ThreeItems", "FourItems", "FiveItems"];

    private protected override string Describe(IEnumerable<TItem>? value, string? methodName = null)
        => value is ICollection<TItem> col && _methodsWithCount.Contains(methodName)
            ? $"{col.Count}: {DescribeAtMostFive(col)}"
            : DescribeAtMostFive(value);

    private protected static string Express<TValue>(string? valueExpr, TValue value)
    {
        var valueStr = value.FormatValue();
        return valueExpr is null || valueExpr == valueStr ? valueStr : $"'{valueExpr.ParseValue()}' = {value}";
    }

    private static string DescribeAtMostFive(IEnumerable<TItem>? value) 
        => value?.Count() > 5
            ? '[' + string.Join(", ", value.Take(4).Select(it => it.FormatValue()).Append("...")) + ']'
            : value.FormatValue();

    private protected static Action<IEnumerable<TItem>?> NotEmptyAnd(Action<IEnumerable<TItem>> assert)
        => actual =>
        {
            Xunit.Assert.NotNull(actual);
            Xunit.Assert.NotEmpty(actual);
            assert(actual);
        };
}