using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture;
using TSpec.Architecture.CSharp;
using TSpec.Assert;

namespace MyHotel.Architecture.Spec.Layout;

/// <summary>CODING-STANDARD-BACKEND.md 11b as an invariant over the syntax trees: a list of arguments or
/// parameters is on one line, at or below the line that opens it, or has each item on a line of its own below
/// that line, a tuple type's elements too; a tuple value opens with its first item and closes with its
/// last.</summary>
public class ArgumentLists : TSpec.Spec
{
    public static TheoryData<string> Projects => Sources.Projects;

    [Theory, MemberData(nameof(Projects))]
    public void AListIsOnOneLineOrOneItemPerLine(string project) =>
        Source.Projects[project].Files.SelectMany(BadlyListed).Is().Empty();

    /// <summary>"file:line" for every list that is neither on one line nor one item per line below its
    /// opening line, at the line it opens on.</summary>
    private static IEnumerable<string> BadlyListed(SourceFile file) =>
        file.Root.DescendantNodes().Select(Items).OfType<Listed>()
            .Where(list => list.Items.Count > 1 && !Shaped(list, file))
            .Select(list => $"{file}:{file.Line(list.Open)}");

    /// <summary>All items on one line, or each on its own line: below the opening token's for a headed
    /// list, from the opening token's for a tuple.</summary>
    private static bool Shaped(Listed list, SourceFile file)
    {
        IReadOnlyList<int> lines = [.. list.Items.Select(item => file.Line(item)).Distinct()];
        var opens = file.Line(list.Open);
        var ownLines = list.Headless ? lines[0] == opens : lines[0] > opens;
        return lines.Count == 1 || (lines.Count == list.Items.Count && ownLines);
    }

    private static Listed? Items(SyntaxNode node) =>
        node switch
        {
            ArgumentListSyntax list => new(list.OpenParenToken, [.. list.Arguments], Headless: false),
            ParameterListSyntax list => new(list.OpenParenToken, [.. list.Parameters], Headless: false),
            TupleExpressionSyntax tuple => new(tuple.OpenParenToken, [.. tuple.Arguments], Headless: true),
            TupleTypeSyntax tuple => new(tuple.OpenParenToken, [.. tuple.Elements], Headless: false),
            _ => null,
        };

    /// <summary>One list: its opening token, its items, and whether it is a tuple value (headless).</summary>
    private sealed record Listed(SyntaxToken Open, IReadOnlyList<SyntaxNode> Items, bool Headless);
}
