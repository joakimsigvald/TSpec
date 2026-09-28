using TSpec.Assert;

namespace TSpec.Test.Pipeline;

public interface ICounter
{
    int Next();
}

public class Tally(ICounter counter)
{
    public static int Runs;

    public int Count()
    {
        Runs++;
        return counter.Next();
    }
}

/// Starting with a Then or a Because reads the outcome, so the Fact shares.
public class WhenAFactStartsWithThen : Spec<Tally, int>
{
    public WhenAFactStartsWithThen()
        => When(_ => _.Count()).Given<ICounter>().That(_ => _.Next()).Returns(() => 0);

    [Fact]
    public void ThenOneVerifiesTheMock()
    {
        Then<ICounter>(_ => _.Next());
        Tally.Runs.Is(1);
    }

    [Fact]
    public void ThenAnotherChecksTheResult()
    {
        Then().Result.Is(0);
        Tally.Runs.Is(1);
    }

    [Fact]
    public void ThenAThirdGivesAReason()
    {
        Because("nothing was counted").Result.Is(0);
        Tally.Runs.Is(1);
        Specification.Does().Contain("because nothing was counted");
    }
}
