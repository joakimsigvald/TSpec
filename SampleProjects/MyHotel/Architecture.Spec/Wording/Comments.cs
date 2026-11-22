using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 9c and 9f as invariants over the comments: a type or a member is
/// described by a `///` block of at most ten lines, never by a `//` comment - a field may have one - a `//`
/// comment runs at most three lines, and there is no `/* */` comment, nor its `/** */` twin. `//` lines in a
/// row are one comment, but a note after code on its line is a comment of its own.</summary>
public class Comments : TSpec.Spec
{
    private const int MaxSummaryLines = 10;
    private const int MaxCommentLines = 3;

    [Fact]
    public void ATypeOrAMemberIsDescribedByASummaryNotAComment() =>
        Source.Projects.Has().All(project => CommentsOf(project).Has().None(DescribesATypeOrAMember));

    [Fact]
    public void ASummaryIsAtMostTenLines() =>
        Source.Projects.Has().All(project => CommentsOf(project).Has().None(IsALongSummary));

    [Fact]
    public void ACommentIsAtMostThreeLines() =>
        Source.Projects.Has().All(project => CommentsOf(project).Has().None(IsALongComment));

    [Fact]
    public void ThereIsNoBlockComment() =>
        Source.Projects.Has().All(project => CommentsOf(project).Has().None(IsABlockComment));

    private static IReadOnlyList<SourceComment> CommentsOf(SourceProject project) =>
        [.. project.Files.SelectMany(file => file.Comments)];

    private static bool DescribesATypeOrAMember(SourceComment comment) =>
        comment.Kind is CommentKind.Line
        && comment.Leads is MemberDeclarationSyntax member
        && IsDescribed(member);

    private static bool IsALongSummary(SourceComment comment) =>
        comment.Kind is CommentKind.Documentation && comment.Span.Lines > MaxSummaryLines;

    private static bool IsALongComment(SourceComment comment) =>
        comment.Kind is CommentKind.Line && comment.Span.Lines > MaxCommentLines;

    private static bool IsABlockComment(SourceComment comment) =>
        comment.Kind is CommentKind.Block or CommentKind.DocumentationBlock;

    /// <summary>A type or a member a summary describes - not a namespace, a field, an enum's value or a
    /// top-level statement.</summary>
    private static bool IsDescribed(MemberDeclarationSyntax member) =>
        member is not (BaseNamespaceDeclarationSyntax
            or BaseFieldDeclarationSyntax
            or EnumMemberDeclarationSyntax
            or GlobalStatementSyntax
            or IncompleteMemberSyntax);
}
