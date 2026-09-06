namespace TSpec.Internal.Document;

/// <summary>
/// Where something is written: the file the compiler recorded, and the line — none when only the
/// file is known, as for a class that writes no constructor.
/// </summary>
internal sealed record SourceLocation(string File, int? Line);
