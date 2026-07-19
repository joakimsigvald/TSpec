using System.Collections;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    private const int _maxElements = 5;
    private const int _maxElementLength = 50;

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
        var elements = col.Cast<object?>().Take(_maxElements + 1).Select(FormatElement).ToList();
        if (elements.Count > _maxElements)
            elements[_maxElements] = "...";
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
        => text.Length <= _maxElementLength ? text : $"{text[.._maxElementLength]}...";
}
