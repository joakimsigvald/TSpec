namespace TSpec.Internal.Specification;

/// <summary>
/// Break-point markers, written into described text while composition still knows the structure
/// and read back by <see cref="TextBuilder"/> when a line must break. <see cref="Enter"/> and
/// <see cref="Exit"/> bracket a nested construct; <see cref="Point"/> marks a break opportunity,
/// ranked by its nesting depth — a line breaks at the last point of the shallowest rank that
/// fits. The markers are never part of the text: layout strips them while measuring, and any
/// path that hands described text to output directly must strip them itself.
/// </summary>
internal static class Wrap
{
    internal const char Enter = '\u0001';
    internal const char Exit = '\u0002';
    internal const char Point = '\u0003';

    internal static char[] Markers { get; } = [Enter, Exit, Point];

    internal static string StripWrapMarkers(this string text)
        => text.IndexOfAny(Markers) < 0
            ? text
            : string.Concat(text.Where(c => !Markers.Contains(c)));
}
