using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Wording;

/// <summary>CODING-STANDARD-BACKEND.md 9c and 9f as invariants over the syntax trees: a type or a member is
/// described by a `///` block of at most ten lines, never by a `//` comment - a field may have one - a `//`
/// comment runs at most three lines, and there is no `/* */` comment, nor its `/** */` twin. One fact per
/// project (Sources); a breach is counted.</summary>
public class Comments : TSpec.Spec
{
    private const int MaxSummaryLines = 10;
    private const int MaxCommentLines = 3;

    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void ATypeOrAMemberIsDescribedByASummaryNotAComment(string project) =>
        Breach(project, DescribedByAComment, "types or members carry a // comment").Is().Null();

    [Theory, MemberData(nameof(Projects))]
    public void ASummaryIsAtMostTenLines(string project) =>
        Breach(project, LongSummaries, "/// blocks run over ten lines").Is().Null();

    [Theory, MemberData(nameof(Projects))]
    public void ACommentIsAtMostThreeLines(string project) =>
        Breach(project, LongComments, "// comments run over three lines").Is().Null();

    [Theory, MemberData(nameof(Projects))]
    public void ThereIsNoBlockComment(string project) =>
        Breach(project, BlockComments, "/* */ comments").Is().Null();

    private static string? Breach(
        string project,
        Func<SourceFile, IEnumerable<string>> sitesIn,
        string what) =>
        Sites(project, sitesIn) is [var first, ..] and var sites
            ? $"{sites.Count} {what}, the first at {first}"
            : null;

    private static IReadOnlyList<string> Sites(
        string project,
        Func<SourceFile, IEnumerable<string>> sitesIn) =>
        [.. Source.Projects[project].Files.SelectMany(sitesIn)];

    /// <summary>"file:line" for every type or member with a comment over it.</summary>
    private static IEnumerable<string> DescribedByAComment(SourceFile file) =>
        file.Root.DescendantNodes().OfType<MemberDeclarationSyntax>()
            .Where(IsDescribed)
            .Where(member => member.GetLeadingTrivia().Any(IsComment))
            .Select(member => $"{file}:{file.Line(member)}");

    /// <summary>"file:line" for every `///` block over the limit.</summary>
    private static IEnumerable<string> LongSummaries(SourceFile file) =>
        file.Root.DescendantTrivia()
            .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Where(summary => SummaryLinesOf(summary) > MaxSummaryLines)
            .Select(summary => $"{file}:{LinesOf(summary, file).First()}");

    /// <summary>"file:line" for the first line of every run of comment lines over the limit.</summary>
    private static IEnumerable<string> LongComments(SourceFile file) =>
        RunsOverTheLimit(CommentLinesOf(file)).Select(line => $"{file}:{line}");

    private static IEnumerable<string> BlockComments(SourceFile file) =>
        file.Root.DescendantTrivia().Where(IsABlockComment)
            .Select(comment => $"{file}:{LinesOf(comment, file).First()}");

    /// <summary>A type or a member a summary describes - not a namespace, a field, an enum's value or a
    /// top-level statement.</summary>
    private static bool IsDescribed(MemberDeclarationSyntax member) =>
        member is not (BaseNamespaceDeclarationSyntax
            or BaseFieldDeclarationSyntax
            or EnumMemberDeclarationSyntax
            or GlobalStatementSyntax
            or IncompleteMemberSyntax);

    private static bool IsComment(SyntaxTrivia trivia) => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia);

    /// <summary>A comment beside code on its line is no line of a comment run: lines of code, each with a
    /// short note at its end, are not one long comment.</summary>
    private static bool StandsOnItsOwnLine(SyntaxTrivia comment) =>
        !comment.Token.TrailingTrivia.Contains(comment);

    private static bool IsABlockComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
        || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);

    private static int SummaryLinesOf(SyntaxTrivia summary) =>
        summary.ToFullString().Split('\n').Count(IsASummaryLine);

    private static bool IsASummaryLine(string line) =>
        line.TrimStart().StartsWith("///", StringComparison.Ordinal);

    /// <summary>The first line of every run of consecutive lines longer than the limit.</summary>
    private static IEnumerable<int> RunsOverTheLimit(IReadOnlySet<int> lines) =>
        lines.Where(line => !lines.Contains(line - 1))
            .Where(start => Enumerable.Range(start, MaxCommentLines + 1).All(lines.Contains));

    /// <summary>Every line a comment covers, counted from 1.</summary>
    private static IReadOnlySet<int> CommentLinesOf(SourceFile file)
    {
        HashSet<int> lines =
            [.. file.Root.DescendantTrivia().Where(IsComment).Where(StandsOnItsOwnLine)
                .SelectMany(comment => LinesOf(comment, file))];
        return lines;
    }

    /// <summary>The lines a piece of trivia covers, counted from 1.</summary>
    private static IEnumerable<int> LinesOf(SyntaxTrivia trivia, SourceFile file) =>
        LinesOf(file.Root.SyntaxTree.GetLineSpan(trivia.Span));

    private static IEnumerable<int> LinesOf(FileLinePositionSpan span) =>
        Enumerable.Range(span.StartLinePosition.Line + 1, LineCountOf(span));

    private static int LineCountOf(FileLinePositionSpan span) =>
        span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
}
