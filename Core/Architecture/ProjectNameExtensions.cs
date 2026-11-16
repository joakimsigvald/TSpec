namespace TSpec.Architecture;

/// <summary>
/// Project names read in segments, for rules that switch on a layer rather than on a project
/// </summary>
[Obsolete(Source.MovedToCSharp)]
public static class ProjectNameExtensions
{
    /// <summary>
    /// The segment of a dotted project name at the given index, or empty where it has none:
    /// <c>"Formats.Fhir".Segment(0)</c> is "Formats" and <c>"Formats.Fhir".Segment(^1)</c> is "Fhir"
    /// </summary>
    /// <param name="name">The project name</param>
    /// <param name="index">The index of the segment, from the end with ^</param>
    /// <returns>The segment, or empty</returns>
    [Obsolete(Source.MovedToCSharp)]
    public static string Segment(this string name, Index index) => NameSegments.At(name, index);

    /// <summary>
    /// The segments of a dotted project name in the given range, joined by dots, or empty where it
    /// has none: <c>"Data.Auth.Store".Segment(..2)</c> is "Data.Auth"
    /// </summary>
    /// <param name="name">The project name</param>
    /// <param name="range">The range of segments</param>
    /// <returns>The segments, or empty</returns>
    [Obsolete(Source.MovedToCSharp)]
    public static string Segment(this string name, Range range) => NameSegments.In(name, range);
}

/// The segments of a dotted name, for TSpec's own reading of the build while Segment is deprecated.
internal static class NameSegments
{
    internal static string At(string name, Index index)
    {
        var segments = name.Split('.');
        var offset = index.GetOffset(segments.Length);
        return offset >= 0 && offset < segments.Length ? segments[offset] : string.Empty;
    }

    internal static string In(string name, Range range)
    {
        var segments = name.Split('.');
        var start = range.Start.GetOffset(segments.Length);
        var end = range.End.GetOffset(segments.Length);
        return start >= 0 && start <= end && end <= segments.Length
            ? string.Join('.', segments[start..end])
            : string.Empty;
    }
}
