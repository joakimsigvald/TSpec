using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheLines : Spec
{
    [Fact]
    public void ThenGiveEachLineItsTextAsAnEditorCountsThem()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\r\nb\n\nc\n").SourceLines.Select(line => line.Text).Is().EqualTo(["a", "b", "", "c"]);
    }

    [Fact]
    public void ThenNumberThemFromOne()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\nb\nc").SourceLines.Select(line => line.Number).Is().EqualTo([1, 2, 3]);
    }

    [Fact]
    public void GivenAnEmptyFile_ThenItHasOneEmptyLine()
    {
        using var project = new TempProject("Entry");
        Written(project, "").SourceLines.Select(line => line.Text).Is().EqualTo([""]);
    }

    [Fact]
    public void ThenEachLineKnowsItsFile()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "a\nb");
        file.SourceLines[1].File.Is(file);
    }

    [Fact]
    public void ThenNameALineByItsFileAndNumber()
    {
        using var project = new TempProject("Entry");
        Written(project, "a\nb").SourceLines[1].ToString().Is("Entry/Rooms/RoomEndpoints.cs:2");
    }

    [Fact]
    public void GivenOnlyCode_ThenEveryLineIsCode()
    {
        using var project = new TempProject("Entry");
        CodeIn(Written(project, "class A\n{\n    string B = \"b\";\n}")).Is().EqualTo([true, true, true, true]);
    }

    [Fact]
    public void GivenARawStringOverLines_ThenTheLinesAfterItsFirstAreNoCode()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "var a = \"\"\"\n    text\n    \"\"\";\nvar b = 1;");
        CodeIn(file).Is().EqualTo([true, false, false, true]);
    }

    [Fact]
    public void GivenAnInterpolatedRawString_ThenItsTextIsNoCodeEither()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "var a = $\"\"\"\n    {1}\n    \"\"\";");
        CodeIn(file).Is().EqualTo([true, false, false]);
    }

    [Fact]
    public void GivenAVerbatimStringOverLines_ThenItsLaterLinesAreNoCode()
    {
        using var project = new TempProject("Entry");
        CodeIn(Written(project, "var a = @\"one\ntwo\";")).Is().EqualTo([true, false]);
    }

    [Fact]
    public void GivenABlockCommentOverLines_ThenItsLaterLinesAreNoCode()
    {
        using var project = new TempProject("Entry");
        CodeIn(Written(project, "/* one\n   two */\nclass A;")).Is().EqualTo([true, false, true]);
    }

    private static IReadOnlyList<bool> CodeIn(SourceFile file) => [.. file.SourceLines.Select(line => line.IsCode)];
}
