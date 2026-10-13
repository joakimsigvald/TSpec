using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenAre : Spec
{
    [Fact]
    public void GivenTheSameGuestsInAnotherOrder_ThenCompletes()
    {
        string[] invited = ["Ann", "Bo"];
        string[] guests = ["Bo", "Ann"];
        guests.Are(invited);
        Specification.Is("Guests are invited");
    }

    [Fact]
    public void GivenAGuestNotInvited_ThenGetException()
    {
        string[] invited = ["Ann", "Bo"];
        string[] guests = ["Bo", "Ann", "Cy"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""Expected guests to be ["Ann", "Bo"] but found 3: ["Bo", "Ann", "Cy"]""", "Guests are invited");
    }

    [Fact]
    public void GivenAnInvitedGuestMissing_ThenGetException()
    {
        string[] invited = ["Ann", "Bo"];
        string[] guests = ["Bo"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""Expected guests to be ["Ann", "Bo"] but found 1: ["Bo"]""", "Guests are invited");
    }
}
