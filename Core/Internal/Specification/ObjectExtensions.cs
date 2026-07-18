using System.Collections;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    internal static string FormatValue(this object? value)
        => value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            IEnumerable col => $"[{string.Join(", ", col.Cast<object?>().Select(FormatValue))}]",
            bool b => b ? "true" : "false",
            _ => value.ToString()
        } ?? "null";
}
