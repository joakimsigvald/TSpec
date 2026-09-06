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
    /// Two columns share the eighty-seven places the indent and the bars leave, so neither may run
    /// past forty. What does not fit is cut, and says that it was.
    /// </summary>
    [Fact]
    public void GivenAValueOutrunsItsShare_ThenCutItAndSaySo()
    {
        var document = Render(Theory(
            ["identifier", "expected"],
            [$"\"{new string('a', 60)}\"", $"\"{new string('b', 60)}\""]));
        document.Does().Contain($"  | \"{new string('a', 38)}… | \"{new string('b', 38)}… |\n");
        document.Split('\n').Where(IsTable).Max(line => line.Length).Is().not.GreaterThan(90);
    }

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
