using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec;

/// <summary>CODING-STANDARD-BACKEND.md 11b where ArgumentLists does not look: a list of arguments or
/// parameters whose items all start on one line is on that line, so its last item ends there too - never
/// hugged, wrapping below it. One fact per project (Sources).</summary>
public class ListEnds : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AListWhoseItemsStartOnOneLineEndsOnIt(string project) =>
        Source.Projects[project].Files.SelectMany(ListsEndingBelowTheirLine).Is().Empty();

    /// <summary>"file:line" for every list whose items start on one line while its last item ends below
    /// it.</summary>
    private static IEnumerable<string> ListsEndingBelowTheirLine(SourceFile file) =>
        file.Root.DescendantNodes().Select(ItemsOf).OfType<IReadOnlyList<SyntaxNode>>()
            .Where(items => items.Count > 1 && Hugged(items, file))
            .Select(items => $"{file}:{file.Line(items[0])}");

    private static bool Hugged(IReadOnlyList<SyntaxNode> items, SourceFile file) =>
        StartOnOneLine(items, file) && file.End(items[^1]) > file.Line(items[0]);

    private static bool StartOnOneLine(IReadOnlyList<SyntaxNode> items, SourceFile file) =>
        items.All(item => file.Line(item) == file.Line(items[0]));

    private static IReadOnlyList<SyntaxNode>? ItemsOf(SyntaxNode node) =>
        node switch
        {
            ArgumentListSyntax list => [.. list.Arguments],
            ParameterListSyntax list => [.. list.Parameters],
            _ => null,
        };
}
