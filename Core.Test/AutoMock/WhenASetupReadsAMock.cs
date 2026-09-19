using TSpec.Assert;

namespace TSpec.Test.AutoMock;

public class Ticket
{
    public string Owner { get; set; } = "";
}

public interface ITicketStore
{
    Ticket Get();
}

public class TicketPrinter(IIdSource source, ITicketStore store)
{
    public string Print(Ticket ticket) => $"{ticket.Owner} by {source.Name}";
    public string PrintStored() => Print(store.Get());
}

/// Values are arranged before mocks are set up, so a value's setup reading a mock would get what the mock answers unarranged.
public class WhenASetupReadsAMock : Spec<TicketPrinter, string>
{
    [Fact]
    public void GivenTheValuesAreArranged_ThenItIsRefused()
        => Xunit.Assert.Throws<SetupFailed>(() =>
            When(_ => _.Print(The<Ticket>()))
                .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
                .Given().A<Ticket>(t => t.Owner = The<IIdSource>().Name)
                .Then())
            .Message.Is(
                "IIdSource.Name is read before IIdSource is set up. Share a value instead: "
                + "Given<IIdSource>().That(_ => _.Name).Returns(() => The<string>()), "
                + "and use The<string>() in the setup");

    [Fact]
    public void GivenTheSuggestedSharedValue_ThenBothGetIt()
        => When(_ => _.Print(The<Ticket>()))
            .Given<IIdSource>().That(_ => _.Name).Returns(() => The<string>())
            .Given().A<Ticket>(t => t.Owner = The<string>())
            .Then().Result.Is($"{The<string>()} by {The<string>()}");

    [Fact]
    public void GivenTheSetupRunsInTheAct_ThenTheMockIsSetUpAndAnswers()
        => When(_ => _.PrintStored())
            .Given<IIdSource>().That(_ => _.Name).Returns(() => "arranged")
            .And<ITicketStore>().That(_ => _.Get()).Returns(() => A<Ticket>(t => t.Owner = The<IIdSource>().Name))
            .Then().Result.Is("arranged by arranged");
}
