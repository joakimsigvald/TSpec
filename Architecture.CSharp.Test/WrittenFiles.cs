using Microsoft.CodeAnalysis.CSharp;
using TSpec.Test.Internal.Document;

namespace TSpec.Architecture.CSharp.Test;

/// <summary>
/// A source file written for a spec, in a project of its own that no build compiled.
/// </summary>
internal static class WrittenFiles
{
    public static SourceFile Written(TempProject project, string text)
    {
        var location = project.AddSource("Rooms", "RoomEndpoints.cs");
        File.WriteAllText(location, text);
        return new("Entry", "Rooms/RoomEndpoints.cs", location, CSharpParseOptions.Default);
    }
}
