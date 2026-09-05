using TSpec.Assert;
using TSpec.Internal.Document;

namespace TSpec.Test.Internal.Document;

/// <summary>
/// A theory parameter is a hole in the document, not a value: every row composes the same claim,
/// which is what lets one bullet stand over a table of rows. The value stays in the per-test
/// specification, where a failing row still has to say what it was given.
/// </summary>
public class WhenATheoryStatesItsRows : Spec
{
    private const int Width = 90;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ThenComposeTheSameClaimWhateverTheRowIs(int count)
    {
        int[] numbers = [.. Enumerable.Range(1, count)];
        numbers.Has().Count(count);
        Claim().Is("Numbers has count 'count'");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ThenKeepTheValueInThePerTestSpecification(int count)
    {
        int[] numbers = [.. Enumerable.Range(1, count)];
        numbers.Has().Count(count);
        Specification.Is($"Numbers has count 'count' = {count}");
    }

    private string Claim() => Requirement.From([Reported()]).Single().Claim.Fit(Width);
}
