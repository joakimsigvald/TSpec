namespace TSpec.Internal.Specification;

/// <summary>
/// Break-point markers, written into described text while composition still knows the structure
/// and read back by <see cref="TextBuilder"/> when a line must break. <see cref="Enter"/> and
/// <see cref="Exit"/> bracket a group — a construct that stays on one line or breaks at every one
/// of its own break points. What follows a <see cref="Point"/> moves one step in from the line its
/// group opened on; what follows an <see cref="Align"/> lines up with that line. The markers are
/// never part of the text: layout strips them while measuring, and any path that hands described
/// text to output directly must strip them itself.
/// </summary>
internal static class Wrap
{
    internal const char Enter = '\u0001';
    internal const char Exit = '\u0002';
    internal const char Point = '\u0003';
    internal const char Align = '\u0006';

    internal static char[] Markers { get; } = [Enter, Exit, Point, Align];

    internal static string StripWrapMarkers(this string text)
        => text.IndexOfAny(Markers) < 0
            ? text
            : string.Concat(text.Where(c => !Markers.Contains(c)));
}
