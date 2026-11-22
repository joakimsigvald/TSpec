using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Assert;
using TSpec.Test.Internal.Document;
using static TSpec.Architecture.CSharp.Test.WrittenFiles;

namespace TSpec.Architecture.CSharp.Test;

public class WhenReadTheComments : Spec
{
    [Fact]
    public void GivenALineComment_ThenGiveItsKindTextAndPlace()
    {
        using var project = new TempProject("Entry");
        var comment = Written(project, "class A;\n// why\nclass B;").Comments.Single();
        comment.Kind.Is(CommentKind.Line);
        comment.Text.Is("// why");
        comment.Span.Is(new SourceSpan(new(2, 1), new(2, 7)));
    }

    [Fact]
    public void ThenNameACommentByItsFileAndFirstLine()
    {
        using var project = new TempProject("Entry");
        Written(project, "class A;\n// why\nclass B;").Comments.Single().ToString().Is("Entry/Rooms/RoomEndpoints.cs:2");
    }

    [Fact]
    public void GivenLineCommentsOnConsecutiveLines_ThenTheyAreOneComment()
    {
        using var project = new TempProject("Entry");
        var comment = Written(project, "// one\n// two\n// three\nclass A;").Comments.Single();
        comment.Span.Lines.Is(3);
        comment.Text.Is("// one\n// two\n// three");
    }

    [Fact]
    public void GivenAnEmptyLineBetween_ThenTheyAreTwo()
    {
        using var project = new TempProject("Entry");
        Written(project, "// one\n\n// two\nclass A;").Comments.Has().Count(2);
    }

    [Fact]
    public void GivenACommentAfterCodeOnItsLine_ThenItIsTrailingAndStandsAlone()
    {
        using var project = new TempProject("Entry");
        var comments = Written(project, "class A; // note\n// why\nclass B;").Comments;
        comments.Select(comment => comment.IsTrailing).Is().EqualTo([true, false]);
    }

    [Fact]
    public void GivenADocumentationComment_ThenItCoversItsLines()
    {
        using var project = new TempProject("Entry");
        var comment = Written(project, "/// <summary>\n/// A.\n/// </summary>\nclass A;").Comments.Single();
        comment.Kind.Is(CommentKind.Documentation);
        comment.Span.Lines.Is(3);
    }

    [Fact]
    public void GivenABlockComment_ThenItCoversItsLines()
    {
        using var project = new TempProject("Entry");
        var comment = Written(project, "/* one\n   two */\nclass A;").Comments.Single();
        comment.Kind.Is(CommentKind.Block);
        comment.Span.Lines.Is(2);
    }

    [Fact]
    public void GivenADocumentationBlock_ThenItIsOneOfItsOwnKind()
    {
        using var project = new TempProject("Entry");
        Written(project, "/** A. */\nclass A;").Comments.Single().Kind.Is(CommentKind.DocumentationBlock);
    }

    [Fact]
    public void GivenACommentAboveAMember_ThenItLeadsTheMemberItsAttributesIncluded()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "class A\n{\n    // why\n    [Obsolete]\n    void M() { }\n}");
        file.Comments.Single().Leads.Is().A<MethodDeclarationSyntax>();
    }

    [Fact]
    public void GivenACommentAboveATypeAtTheTopOfTheFile_ThenItLeadsTheType()
    {
        using var project = new TempProject("Entry");
        Written(project, "// why\nclass A;").Comments.Single().Leads.Is().A<ClassDeclarationSyntax>();
    }

    [Fact]
    public void GivenACommentAboveAStatement_ThenItLeadsTheStatement()
    {
        using var project = new TempProject("Entry");
        var file = Written(project, "class A\n{\n    void M()\n    {\n        // why\n        M();\n    }\n}");
        file.Comments.Single().Leads.Is().A<ExpressionStatementSyntax>();
    }

    [Fact]
    public void GivenATrailingComment_ThenItLeadsNothing()
    {
        using var project = new TempProject("Entry");
        Written(project, "class A; // note").Comments.Single().Leads.Is().Null();
    }

    [Fact]
    public void GivenAProject_ThenListTheCommentsOfAllItsFiles()
        => Source.Projects["Test"].Comments.Select(comment => comment.ToString()).Does().Contain("Test/WrittenFiles.cs:6");
}
