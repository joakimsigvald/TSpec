using TSpec.Assert;
using TSpec.Internal.Document;
using TSpec.Internal.Document.RenderPipeline;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// A theory states one requirement over a table of its rows: the claim says what holds for every
/// row, and the table says what each row filled the holes with. A table cannot wrap the way prose
/// does, so the columns share the page equally and a value too long for its share is cut.
/// </summary>
public class WhenRenderTheoryRows : Spec
{
    private static readonly SpecificationSubject _myHotel = new("MyHotel", "0.1.0");

    [Fact]
    public void ThenWriteTheRowsAsATableUnderTheRequirement()
        => Render(Theory(["count", "numbers"], ["1", "[1]"], ["2", "[1, 3]"]))
            .Does().Contain(
                """
                - **count** — `has count 'count'`

                  | count | numbers |
                  | ----- | ------- |
                  | 1     | [1]     |
                  | 2     | [1, 3]  |

                """);

    /// <summary>
    /// The table stands directly under the bullet, ahead of a claim that needs a fence of its own:
    /// what the rows were is read before what is claimed of them.
    /// </summary>
    [Fact]
    public void GivenTheClaimNeedsAFence_ThenWriteTheTableAboveIt()
        => Render([
            .. Theory(["count"], [["1"], ["2"]], condition: "post"),
            Entry("ThenOther", "then other")])
            .Does().Contain(
                """
                - **count**

                  | count |
                  | ----- |
                  | 1     |
                  | 2     |

                  ```
                  Having post
                  Then has count 'count'
                  ```

                """);

    /// Rows report in whatever order a parallel run finished them; the table takes the declared one.
    [Fact]
    public void ThenWriteTheRowsInDeclarationOrder()
        => Render(Row(1, ["n"], ["second"]), Row(0, ["n"], ["first"]))
            .Does().Contain("  | n      |\n  | ------ |\n  | first  |\n  | second |\n");

    /// A bar would end the cell it stands in, so a value that holds one says so.
    [Fact]
    public void GivenAValueHoldsABar_ThenEscapeIt()
        => Render(Theory(["text"], ["\"a|b\""]))
            .Does().Contain("  | \"a\\|b\" |\n");

    /// <summary>
    /// Two columns that both want more than the page holds divide what there is between them, and
    /// what does not fit is cut and says that it was. The odd place goes to the last column settled
    /// rather than being left unused.
    /// </summary>
    [Fact]
    public void GivenAValueOutrunsItsShare_ThenCutItAndSaySo()
    {
        var document = Render(Theory(
            ["identifier", "expected"],
            [$"\"{new string('a', 60)}\"", $"\"{new string('b', 60)}\""]));
        document.Does().Contain($"  | \"{new string('a', 38)}… | \"{new string('b', 39)}… |\n");
        document.Split('\n').Where(IsTable).Max(line => line.Length).Is().not.GreaterThan(90);
    }

    /// <summary>
    /// A column takes what it needs and no more, so what it leaves goes to the column that has more
    /// to say. Equal shares would cut the message at forty while a one-character count kept
    /// thirty-nine places it has no use for.
    /// </summary>
    [Fact]
    public void GivenAColumnNeedsLittle_ThenLeaveTheRestToTheOthers()
    {
        var document = Render(Theory(["n", "message"], ["1", $"\"{new string('m', 100)}\""]));
        document.Does().Contain($"  | 1 | \"{new string('m', 78)}… |\n");
        document.Split('\n').Where(IsTable).Max(line => line.Length).Is().not.GreaterThan(90);
    }

    /// <summary>
    /// What every column needs together fits, so nothing is cut and the table is as wide as its
    /// content rather than as wide as the page.
    /// </summary>
    [Fact]
    public void GivenEveryColumnFits_ThenCutNothing()
        => Render(Theory(["n", "word"], ["1", "\"short\""]))
            .Does().Contain("  | n | word    |\n").and.not.Contain("…");

    /// <summary>
    /// Eight columns is what the page holds while every one of them stays readable. A theory with
    /// more parameters than that has outgrown a table, and the document says so rather than
    /// printing a row nobody can read.
    /// </summary>
    [Fact]
    public void GivenMoreColumnsThanTheTableHolds_ThenFail()
    {
        var headers = Enumerable.Range(1, 9).Select(n => $"p{n}").ToArray();
        var error = Xunit.Assert.Throws<SetupFailed>(
            () => Render(Theory(headers, [.. headers.Select(_ => "1")])));
        error.Message.Does().Contain("9").and.Contain("8").and.Contain("p1");
    }

    [Fact]
    public void GivenTheMostColumnsTheTableHolds_ThenRenderThem()
    {
        var headers = Enumerable.Range(1, 8).Select(n => $"p{n}").ToArray();
        Render(Theory(headers, [.. headers.Select(_ => "1")]))
            .Does().Contain("| p1 | p2 | p3 | p4 | p5 | p6 | p7 | p8 |");
    }

    /// <summary>
    /// A cut cell still shows something of what it held: no column is narrower than five, even when
    /// every one of them wants the whole page. The column limit is what makes that hold — eight
    /// columns leave the narrowest seven places — so this says the two settings agree.
    /// </summary>
    [Fact]
    public void ThenLeaveEveryColumnAtLeastFivePlaces()
    {
        var headers = Enumerable.Range(1, 8).Select(n => $"p{n}").ToArray();
        var document = Render(Theory(headers, [.. headers.Select(_ => new string('x', 40))]));
        document.Split('\n').Where(IsTable).Max(line => line.Length).Is().not.GreaterThan(90);
        Widths(document).Min().Is().not.LessThan(5);
    }

    /// The separator row states each column's width as a run of dashes.
    private static int[] Widths(string document)
        => [.. document.Split('\n').First(line => line.StartsWith("  | ---", StringComparison.Ordinal))
            .Split('|')
            .Select(cell => cell.Trim().Length)
            .Where(width => width > 0)];

    /// <summary>
    /// A table closes its item with a blank line and a heading opens with one, which together would
    /// leave a gap the document leaves nowhere else.
    /// </summary>
    [Fact]
    public void GivenATableEndsASection_ThenLeaveOneBlankLineBeforeTheNext()
        => Render([.. Theory(["count"], [["1"]]), Entry("ThenOther", "then other", subject: "WhenOther")])
            .Does().not.Contain("\n\n\n");

    [Fact]
    public void GivenTheRequirementIsNoTheory_ThenWriteNoTable()
        => Render(Entry("ThenCount", "then has count 2"))
            .Does().not.Contain("|");

    private static bool IsTable(string line) => line.StartsWith("  |", StringComparison.Ordinal);

    private static SpecificationEntry[] Theory(
        string[] headers, string[][] rows, string? condition = null)
        => [.. rows.Select((values, index) => Row(index, headers, values, condition))];

    private static SpecificationEntry[] Theory(string[] headers, params string[][] rows)
        => Theory(headers, rows, condition: null);

    private static SpecificationEntry Row(
        int index, string[] headers, string[] values, string? condition = null)
        => Entry("ThenCount", "then has count 'count'", condition)
            with { Row = new(index, headers, values) };

    private static SpecificationEntry Entry(
        string name, string claim, string? condition = null, string subject = "WhenCount")
        => new(subject, "", name,
            [.. condition is null ? Array.Empty<SpecificationClause>() : [Condition(condition)],
            Clause(StepLayout.Sentence, StepFamily.None, claim)]);

    private static SpecificationClause Condition(string body)
        => Clause(StepLayout.SentenceOrPhrase, StepFamily.Having, body);

    private static SpecificationClause Clause(StepLayout layout, StepFamily family, string body)
        => new([new SpecificationStep(layout) { Family = family, Body = body }]);

    private static string Render(params SpecificationEntry[] entries)
        => DocumentRenderer.Render(_myHotel, "MyHotel.Spec", entries);
}
