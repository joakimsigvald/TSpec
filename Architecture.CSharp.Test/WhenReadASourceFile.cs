using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

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
    public void ThenTellWhereANodeStartsAndEnds_CountingLinesAndColumnsFromOne()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "class A;\r\n\nclass B\n{\n}\n");
        file.SpanOf(file.Root.Members[1]).Is(new SourceSpan(new(3, 1), new(5, 2)));
    }

    [Fact]
    public void GivenAToken_ThenTellWhereItStartsAndEnds()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "class Room;");
        file.SpanOf(((ClassDeclarationSyntax)file.Root.Members[0]).Identifier).Is(new SourceSpan(new(1, 7), new(1, 11)));
    }

    [Fact]
    public void ThenGiveTheFolderDirectlyInTheProjectThatItIsIn()
        => At("Rooms/Booking/RoomService.cs").Folder.Is("Rooms");

    [Fact]
    public void GivenAFileAtTheProjectsRoot_ThenItsFolderIsEmpty()
        => At("Program.cs").Folder.Is("");

    [Fact]
    public void ThenGiveItsNameWithoutItsExtension()
        => At("Rooms/Booking/RoomService.cs").Name.Is("RoomService");

    [Fact]
    public void GivenANameWithDots_ThenOnlyTheLastPartIsTheExtension()
        => At("Declared/APartial.More.cs").Name.Is("APartial.More");

    [Fact]
    public void ThenGiveItsExtensionWithoutTheDot()
        => At("Rooms/Booking/RoomService.cs").Extension.Is("cs");

    private static SourceFile At(string path) => new("Entry", path, path, CSharpParseOptions.Default);
}
