using Microsoft.CodeAnalysis.CSharp;
using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Test.Internal.Document;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadASourceFile : Spec
{
    [Fact]
    public void ThenKeepItsTextAsItIsWritten()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\r\nb\n").Raw.Is("a\r\nb\n");
    }

    [Fact]
    public void ThenCountItsLinesAsAnEditorDoes()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\r\nb\n\nc\n").Lines.Is().EqualTo(["a", "b", "", "c"]);
    }

    [Fact]
    public void GivenNoFinalLineBreak_ThenTheLastLineCountsToo()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\nb").Lines.Is().EqualTo(["a", "b"]);
    }

    [Fact]
    public void GivenAnEmptyFile_ThenItHasOneEmptyLine()
    {
        using var project = new TempProject("Entry");
        Written(project, "").Lines.Is().EqualTo([""]);
    }

    [Fact]
    public void ThenNameItByItsProjectAndPath()
    {
        using var project = new TempProject("Entry");
        Written(project, "").ToString().Is("Entry/Rooms/RoomEndpoints.cs");
    }

    [Fact]
    public void ThenTellTheLineANodeStartsOn()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "class A;\r\n\nclass B;\n");
        file.Line(file.Root.Members[1]).Is(3);
    }

    private static SourceFile Written(TempProject project, string text)
    {
        var location = project.AddSource("Rooms", "RoomEndpoints.cs");
        File.WriteAllText(location, text);
        return new("Entry", "Rooms/RoomEndpoints.cs", location, CSharpParseOptions.Default);
    }
}
