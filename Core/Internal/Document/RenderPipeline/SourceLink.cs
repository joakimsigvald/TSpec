namespace TSpec.Internal.Document.RenderPipeline;

/// <summary>
/// The markdown link from a heading to the file it renders, by a path relative to the spec
/// project. A file outside the project has no path every reader of the document shares, so it is
/// not linked. The line is known but not written: a <c>#L</c> anchor is followed by GitHub and VS
/// Code and treated as an in-page anchor by Visual Studio's preview, where the click then does
/// nothing — and a file-level link only makes sense for the class that starts the file.
/// </summary>
internal static class SourceLink
{
    internal static string? Href(SourceLocation? at, string? sourceRoot)
    {
        if (at is null || sourceRoot is null)
            return null;
        var relative = Under(sourceRoot, at.File) ?? FoundUnder(sourceRoot, at.File);
        return relative?.Replace('\\', '/');
    }

    internal static string Around(this string? href, string text)
        => href is null ? text : $"[{text}]({href})";

    /// The path relative to the root, when the file is below it.
    private static string? Under(string root, string file)
    {
        var relative = Path.GetRelativePath(root, file);
        return Path.IsPathRooted(relative)
            || relative == ".."
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                ? null
                : relative;
    }

    /// <summary>
    /// A build that maps source paths records the file as <c>/_/…</c> from the repository root,
    /// which is nowhere on disk. It is found under the spec project by its tail: the longest tail
    /// of the recorded path that is a file there.
    /// </summary>
    private static string? FoundUnder(string root, string file)
    {
        var segments = file.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        string? found = null;
        for (var take = 1; take <= segments.Length; take++)
        {
            var tail = Path.Combine(segments[^take..]);
            if (File.Exists(Path.Combine(root, tail)))
                found = tail;
        }
        return found;
    }
}
