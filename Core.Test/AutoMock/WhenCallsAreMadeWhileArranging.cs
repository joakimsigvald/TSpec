using TSpec.Assert;
using Xunit.Sdk;
using static TSpec.Times;

namespace TSpec.Test.AutoMock;

public interface ITally
{
    void Add(int value);
}

public class Tallying
{
    private readonly ITally _tally;

    public Tallying(ITally tally)
    {
        _tally = tally;
        tally.Add(0);
    }

    public void AddPositive(int value)
    {
        if (value > 0)
            _tally.Add(value);
    }
}

/// Creating the subject and every Having is arranging; only the calls the act makes are counted.
public class WhenCallsAreMadeWhileArranging : Spec<Tallying>
{
    [Fact]
    public void GivenTheConstructorCallsTheMock_ThenItIsNotCounted()
        => When(_ => _.AddPositive(-1)).Then<ITally>(wasInvoked: Never);

    [Fact]
    public void GivenAHavingCallsTheMock_ThenOnlyTheActIsCounted()
        => When(_ => _.AddPositive(2)).Having(_ => _.AddPositive(1))
            .Then<ITally>(_ => _.Add(Any<int>()), Once);

    [Fact]
    public void GivenAHavingCallsTheMock_ThenACountByNameIsOfTheActOnly()
        => When(_ => _.AddPositive(2)).Having(_ => _.AddPositive(1))
            .Then<ITally>(nameof(ITally.Add), Once);

    [Fact]
    public void GivenOnlyAHavingCallsTheMock_ThenTheVerificationFails()
        => Xunit.Assert.Throws<XunitException>(() =>
            When(_ => _.AddPositive(-1)).Having(_ => _.AddPositive(1))
                .Then<ITally>(_ => _.Add(Any<int>())))
            .Message.Is(
                "Expected ITally.Add(any int) to be invoked at least once but was never invoked"
                + Environment.NewLine + "ITally received no calls");
}
