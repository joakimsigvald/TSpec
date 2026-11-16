using System.Runtime.CompilerServices;
using TSpec.Assert;
using TSpec.Assert.Continuations;
using TSpec.Extensibility;

namespace TSpec.Test.Extensibility;

/// <summary>
/// A constraint written as an extension package writes one, with only what Constraint opens to it.
/// </summary>
public class WhenWriteAConstraint : Spec
{
    [Fact]
    public void GivenItsAssertionHolds_ThenCompletes()
        => new Words("inn", "spa").Is().Short();

    [Fact]
    public void GivenItsAssertionFails_ThenSayWhatWasFound()
    {
        var words = new Words("inn", "hotel");
        Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => words.Is().Short())
            .HasMessage("""Expected words to be short but found 1: ["hotel"]""");
    }

    [Fact]
    public void ThenSpecifyItAsTheAssertionsName()
    {
        var words = new Words("inn", "spa");
        words.Is().Short();
        Specification.Is("Words is short");
    }

    [Fact]
    public void ThenStateTheTableItAdds()
    {
        new Words("inn", "hotel").Is().not.Short();
        Reported().Table!.Rows.Select(row => string.Join(" | ", row)).Is().EqualTo(["inn | 3", "hotel | 5"]);
    }
}

public sealed record Words(params string[] All);

public record IsWords : Constraint<Words, IsWords>
{
    private IReadOnlyList<string> _long = [];

    public ContinueWith<IsWords> Short()
    {
        _long = [.. Actual!.All.Where(word => word.Length > 3)];
        AddTable(["Word", "Letters"], [.. Actual.All.Select(word => new[] { word, $"{word.Length}" })]);
        return Expect(Ignore.Me, _ => Xunit.Assert.Empty(_long));
    }

    protected override string Found(Words? actual, string? methodName) => Counted(_long.Count, _long);
}

public static class WordsAssertions
{
    public static IsWords Is(
        this Words? actual, Ignore _ = default, [CallerArgumentExpression(nameof(actual))] string? actualExpr = null)
        => Constraints.Create<Words, IsWords>(actual, actualExpr!);
}
