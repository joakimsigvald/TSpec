using System.Collections;
using System.Globalization;

namespace TSpec.Internal.Specification;

internal static class ObjectExtensions
{
    private const int MaxElements = 5;
    private const int MaxElementLength = 50;
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
            IFormattable formattable => formattable.ToString(null, _documentCulture),
            _ => Formatted(value)
        };

    /// <summary>
    /// The one place the document's formats are stated. Every date-like type reaches its text
    /// through this culture rather than through a pattern of its own — a type that formats itself,
    /// a record's generated <c>ToString</c> among them, renders its members with it too.
    /// </summary>
    private static readonly CultureInfo _documentCulture = DocumentCulture();

    private static CultureInfo DocumentCulture()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.ShortDatePattern = DatePattern;
        culture.DateTimeFormat.LongTimePattern = TimePattern;
        culture.DateTimeFormat.ShortTimePattern = TimePattern;
        return CultureInfo.ReadOnly(culture);
    }

    /// <summary>
    /// A type that formats itself reads the ambient culture and takes no overload to pass one, so
    /// the thread wears the document's for the length of the call and has its own back after.
    /// </summary>
    private static string Formatted(object value)
    {
        var ambient = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = _documentCulture;
        try
        {
            return value.ToString() ?? string.Empty;
        }
        finally
        {
            CultureInfo.CurrentCulture = ambient;
        }
    }

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
