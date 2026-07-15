using System.Collections;
using System.Linq.Expressions;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    internal static string ParseValue(this object? value)
        => value switch
        {
            null => "null",
            string s => $"\"{value}\"",
            IEnumerable col => $"[{string.Join(", ", col.Cast<object?>().Select(ParseValue))}]",
            bool b => b ? "true" : "false",
            _ => value.ToString()
        } ?? "null";

    internal static string ParseValue<TItem>(this Expression<Func<TItem, bool>> condition)
    {
        var str = condition.ToString();
        return str;
    }
}