using TSpec.Assert;
using TSpec.Internal.Document;
using TSpec.Internal.Specification;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// Every row of a theory describes itself the same way once its values are holes, so all of them
/// answer to one requirement. The document used to keep the first and drop the rest — which is what
/// left a theory stating nothing — and now keeps them all, in the order they were declared.
/// </summary>
public class WhenCollectTheoryRows : Spec
{
    [Fact]
    public void ThenHoldEveryRowUnderOneRequirement()
    {
        var requirements = Requirement.From(Theory(rows: 4)).ToArray();
        requirements.Length.Is(1);
        Rows(requirements[0]).Is("row 0 | row 1 | row 2 | row 3");
    }

    /// Rows report in whatever order a parallel run finished them, so the table cannot take theirs.
    [Fact]
    public void ThenPutTheRowsInDeclarationOrder()
        => Rows(Requirement.From(Theory(rows: 4).AsEnumerable().Reverse()).Single())
            .Is("row 0 | row 1 | row 2 | row 3");

    [Fact]
    public void ThenLeaveWhatIsNotATheoryAlone()
    {
        var requirements = Requirement.From([Entry("ThenA"), Entry("ThenB")]).ToArray();
        requirements.Length.Is(2);
        requirements.All(requirement => requirement.Rows.Count == 0).Is(true);
    }

    /// <summary>
    /// Rows that describe themselves differently are two requirements, which is what a theory looks
    /// like when something it says was never made a hole. It states less than it could, rather than
    /// claiming rows under a sentence that does not cover them.
    /// </summary>
    [Fact]
    public void GivenTheRowsDescribeThemselvesDifferently_ThenKeepThemApart()
        => Requirement.From([Row(0, "then a"), Row(1, "then b")]).Count().Is(2);

    private static SpecificationEntry[] Theory(int rows)
        => [.. Enumerable.Range(0, rows).Select(index => Row(index, "then something"))];

    private static SpecificationEntry Row(int index, string claim)
        => Entry("ThenReadTheRow", claim) with { Row = new(index, ["value"], [$"row {index}"]) };

    private static SpecificationEntry Entry(string name, string claim = "then something")
        => new("WhenReadRows", "", name, [new([
            new SpecificationStep(StepLayout.Sentence) { Family = StepFamily.None, Body = claim }])]);

    private static string Rows(Requirement requirement)
        => string.Join(" | ", requirement.Rows.Select(row => string.Join(",", row.Values)));
}
