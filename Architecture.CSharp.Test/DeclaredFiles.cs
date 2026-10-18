namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// The files in Declared, read from the build.
/// </summary>
internal static class DeclaredFiles
{
#if DEBUG
    public const string Compiled = "ADebugType";
#else
    public const string Compiled = "AReleaseType";
#endif

    public static SourceFile DeclaredFile(string file)
        => Source.Projects["Test"].Files.Single(source => source.Path == $"Declared/{file}");
}
