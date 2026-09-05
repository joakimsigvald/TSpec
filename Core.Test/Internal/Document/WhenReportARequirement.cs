using TSpec.Assert;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// What a passing test hands to the document. A theory hands its row along with its clauses, since
/// the clauses are the same for every row and the row is the only thing that tells them apart.
/// </summary>
public class WhenReportARequirement : Spec
{
    private sealed class InnerSpec : Spec;

    [Theory]
    [InlineData("a", 1)]
    [InlineData("b", 2)]
    public void ThenKeepTheTheoryRow(string text, int number)
    {
        using InnerSpec inner = new();
        var row = inner.Reported().Row!;
        string.Join(", ", row.Headers).Is("text, number");
        string.Join(", ", row.Values).Is($"\"{text}\", {number}");
    }

    [Fact]
    public void ThenKeepNoRowForAFact()
    {
        using InnerSpec inner = new();
        Xunit.Assert.Null(inner.Reported().Row);
    }
}
