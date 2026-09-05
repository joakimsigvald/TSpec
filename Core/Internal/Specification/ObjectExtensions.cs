using System.Collections;
using System.Globalization;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    private const int MaxElements = 5;
    private const int MaxElementLength = 50;
    private const string DateTimePattern = "yyyy-MM-dd HH:mm:ss";
    private const string DateTimeOffsetPattern = "yyyy-MM-dd HH:mm:ss zzz";
    private const string DatePattern = "yyyy-MM-dd";
    private const string TimePattern = "HH:mm:ss";

    internal static string FormatValue(this object? value)
        => value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            IEnumerable col => FormatCollection(col),
            bool b => b ? "true" : "false",
            _ => value.InvariantText()
        };

    /// <summary>
    /// A value's text, free of the machine's culture: the document has to come out byte-identical
    /// wherever the suite runs, and a failure message has to read the same as the document does.
    /// Dates state their parts in descending order, which no locale can read the wrong way round.
    /// </summary>
    internal static string InvariantText(this object? value)
        => value switch
        {
            null => string.Empty,
            DateTime date => date.ToString(DateTimePattern, CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString(DateTimeOffsetPattern, CultureInfo.InvariantCulture),
            DateOnly date => date.ToString(DatePattern, CultureInfo.InvariantCulture),
            TimeOnly time => time.ToString(TimePattern, CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
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
            _ => Cap(element.InvariantText())
        };

    private static string Cap(string text)
        => text.Length <= MaxElementLength ? text : $"{text[..MaxElementLength]}...";
}
