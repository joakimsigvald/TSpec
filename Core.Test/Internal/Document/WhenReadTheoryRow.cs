using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// The row a theory is running, read from the xunit context so the document can lay a theory's
/// data out as a table. Only <c>InlineData</c> is read: data living in a separate file is not
/// specification, and a theory fed from one goes on rendering as it always has.
/// </summary>
public class WhenReadTheoryRow : Spec
{
    [Theory]
    [InlineData("a", 1, 0)]
    [InlineData("b", 2, 1)]
    public void ThenReadHeadersValuesAndIndex(string text, int number, int index)
    {
        var row = TheoryRow.Read()!;
        string.Join(", ", row.Headers).Is("text, number, index");
        string.Join(", ", row.Values).Is($"\"{text}\", {number}, {index}");
        row.Index.Is(index);
    }

    [Theory]
    [InlineData(2, 1, 3)]
    public void ThenCollectAParamsArrayIntoOneValue(int count, params int[] numbers)
    {
        var row = TheoryRow.Read()!;
        string.Join(", ", row.Headers).Is("count, numbers");
        string.Join(", ", row.Values).Is($"{count}, [{string.Join(", ", numbers)}]");
    }

    [Fact]
    public void ThenReadNothingForAFact() => Xunit.Assert.Null(TheoryRow.Read());

    public static TheoryData<int> Numbers => new(1, 2);

    [Theory]
    [MemberData(nameof(Numbers))]
    public void ThenReadNothingForDataFromElsewhere(int number)
    {
        number.Is().GreaterThan(0);
        Xunit.Assert.Null(TheoryRow.Read());
    }
}
