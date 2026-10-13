namespace TSpec.Architecture;

/// <summary>
/// A source file of a project, named by its project and its path in it
/// </summary>
public sealed class SourceFile
{
    private readonly Lazy<string> _raw;
    private readonly Lazy<IReadOnlyList<string>> _lines;

    internal SourceFile(string project, string path, string location)
    {
        Project = project;
        Path = path;
        Location = location;
        _raw = new(() => File.ReadAllText(location));
        _lines = new(() => LinesOf(Raw));
    }

    // Where the debug information says the file is, which is how its types find it.
    internal string Location { get; }

    /// <summary>
    /// The project the file is compiled into, named as the rules see it
    /// </summary>
    public string Project { get; }

    /// <summary>
    /// The file's path in its project, written with '/': "Rooms/RoomService.cs"
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The file's text as it is written, for an analysis TSpec does not have. Add one to the file as an
    /// extension property, such as a syntax tree parsed with Roslyn, and keep what is costly to compute
    /// in a <c>ConditionalWeakTable</c> keyed by the file itself: TSpec hands out the same file every time
    /// </summary>
    public string Raw => _raw.Value;

    /// <summary>
    /// The file's lines as an editor counts them: a final line break ends the last line and starts none,
    /// and an empty file has one empty line
    /// </summary>
    public IReadOnlyList<string> Lines => _lines.Value;

    /// <summary>
    /// The types declared at the file's top level, in the ordinal order of their full names; a nested
    /// type stays with its owner
    /// </summary>
    public IReadOnlyList<DeclaredType> Types { get; internal set; } = [];

    /// <summary>
    /// The file as a failure names it: its project and its path in it
    /// </summary>
    public override string ToString() => $"{Project}/{Path}";

    private static IReadOnlyList<string> LinesOf(string text) => text.Length == 0 ? [""] : [.. ReadLines(new StringReader(text))];

    private static IEnumerable<string> ReadLines(StringReader reader)
    {
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            yield return line;
    }
}
