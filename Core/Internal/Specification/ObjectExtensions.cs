using System.Collections;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    private const int MaxElements = 5;
    private const int MaxElementLength = 50;

    internal static string FormatValue(this object? value)
        => value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            IEnumerable col => FormatCollection(col),
            bool b => b ? "true" : "false",
            _ => value.ToString() ?? "null"
        };

    /// At most five elements are shown, then an ellipsis. Elements are rendered with their
    /// own ToString (capped in length), so records and tuples read naturally without
    /// expanding nested structure.
    private static string FormatCollection(IEnumerable col)
    {
        var elements = col.Cast<object?>().Take(MaxElements + 1).Select(FormatElement).ToList();
        if (elements.Count > MaxElements)
            elements[MaxElements] = "...";
        return $"[{string.Join(", ", elements)}]";
    }

    private static string FormatElement(object? element)
        => element switch
        {
            null => "null",
            string s => $"\"{Cap(s)}\"",
            bool b => b ? "true" : "false",
            _ => Cap(element.ToString() ?? "null")
        };

    private static string Cap(string text)
        => text.Length <= MaxElementLength ? text : $"{text[..MaxElementLength]}...";
}
