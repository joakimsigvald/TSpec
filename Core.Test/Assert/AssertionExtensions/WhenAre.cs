using TSpec.Assert;

namespace TSpec.Test.Assert.AssertionExtensions;

public class WhenAre : Spec
{
    private record Booking(string Guest, int Room, DateOnly Arrival);
    private record Suite(int Number, int[] Beds);

    private static readonly DateOnly _day = new(2026, 10, 14);

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
        ex.HasMessage("""
            Expected guests to be invited
              extra: 2: "Cy"
            """, "Guests are invited");
    }

    [Fact]
    public void GivenAnInvitedGuestMissing_ThenGetException()
    {
        string[] invited = ["Ann", "Bo"];
        string[] guests = ["Bo"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              missing: 0: "Ann"
            """, "Guests are invited");
    }

    [Fact]
    public void GivenAnotherGuestInPlaceOfAnInvitedOne_ThenNameBoth()
    {
        string[] invited = ["Ann", "Bo"];
        string[] guests = ["Bo", "Cy"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              missing: 0: "Ann"
              extra: 1: "Cy"
            """);
    }

    [Fact]
    public void GivenAGuestInvitedTwiceAndFoundOnce_ThenOneIsMissing()
    {
        string[] invited = ["Ann", "Ann", "Bo"];
        string[] guests = ["Bo", "Ann"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              missing: 1: "Ann"
            """);
    }

    [Fact]
    public void GivenSeveralMissingThatFitOnALine_ThenListThemOnIt()
    {
        string[] invited = ["Ann", "Bo", "Cy"];
        string[] guests = ["Bo"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              missing: 0: "Ann", 2: "Cy"
            """);
    }

    [Fact]
    public void GivenADifferenceTooLongForALine_ThenListEachItemOnItsOwn()
    {
        Booking[] booked =
        [
            new("Ann", 101, _day), new("Bo", 102, _day), new("Cy", 103, _day), new("Di", 104, _day),
            new("Ed", 105, _day), new("Flo", 106, _day.AddDays(1)), new("Gus", 107, _day.AddDays(2)),
        ];
        var bookings = new Booking[]
        {
            new("Bo", 102, _day), new("Ann", 101, _day), new("Cy", 103, _day), new("Di", 104, _day),
            new("Ed", 105, _day),
        }.Where(booking => booking.Room > 0);
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => bookings.Are(booked));
        ex.HasMessage("""
            Expected bookings to be booked
              missing:
                5: Booking { Guest = Flo, Room = 106, Arrival = 2026-10-15 }
                6: Booking { Guest = Gus, Room = 107, Arrival = 2026-10-16 }
            """);
    }

    [Fact]
    public void GivenAChangedItem_ThenNameItAsMissingAndExtra()
    {
        Booking[] booked = [new("Ann", 101, _day), new("Flo", 106, _day.AddDays(1))];
        Booking[] bookings = [new("Flo", 106, _day.AddDays(2)), new("Ann", 101, _day)];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => bookings.Are(booked));
        ex.HasMessage("""
            Expected bookings to be booked
              missing: 1: Booking { Guest = Flo, Room = 106, Arrival = 2026-10-15 }
              extra: 0: Booking { Guest = Flo, Room = 106, Arrival = 2026-10-16 }
            """);
    }

    [Fact]
    public void GivenAnItemLongerThan80Characters_ThenCutIt()
    {
        string[] invited = ["Ann"];
        string[] guests = ["Ann", new string('x', 81)];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage($"""
            Expected guests to be invited
              extra:
                1: "{new string('x', 80)}..."
            """);
    }

    [Fact]
    public void GivenItemsAlikeButNotEqual_ThenNameOnlyTheOneThatDiffers()
    {
        Suite[] booked = [new(1, [1, 2]), new(2, [1])];
        Suite[] suites = [new(1, [1, 2]), new(2, [2])];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => suites.Are(booked));
        ex.HasMessage("""
            Expected suites to be booked
              missing: 1: Suite { Number = 2, Beds = System.Int32[] }
              extra: 1: Suite { Number = 2, Beds = System.Int32[] }
            """);
    }

    [Fact]
    public void GivenNoGuests_ThenFoundNull()
    {
        string[] invited = ["Ann"];
        string[]? guests = null;
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("Expected guests to be invited but found null");
    }

    [Fact]
    public void GivenAnEndlessSequenceOfGuests_ThenNameTheExtraOnesAmongOneMoreThanInvited()
    {
        string[] invited = ["Ann", "Bo"];
        var guests = Endless("Ann", "Bo", "Cy");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              extra: 2: "Cy"
            """);
    }

    [Fact]
    public void GivenMoreGuestsThanInvited_ThenNameNoneMissing()
    {
        string[] invited = ["Ann", "Bo"];
        var guests = Endless("Cy", "Di", "Ann");
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              extra: 0: "Cy", 1: "Di"
            """);
    }

    [Fact]
    public void GivenAnEndlessSequenceOfInvited_ThenNameTheMissingOnesAmongOneMoreThanGuests()
    {
        var invited = Endless("Ann", "Cy", "Di");
        string[] guests = ["Bo", "Ann"];
        var ex = Xunit.Assert.Throws<Xunit.Sdk.XunitException>(() => guests.Are(invited));
        ex.HasMessage("""
            Expected guests to be invited
              missing: 1: "Cy", 2: "Di"
            """);
    }

    [Fact]
    public void GivenTwoSequencesThatAreNotCollections_ThenSetupFails()
    {
        var invited = Endless("Ann");
        var guests = Endless("Ann");
        var ex = Xunit.Assert.Throws<SetupFailed>(() => guests.Are(invited));
        ex.Message.Is("Neither guests nor invited is a collection, so either may never end: make one of them a list or an array");
    }

    // Ends in an exception rather than never, so reading too far fails the test instead of hanging the suite.
    private static IEnumerable<string> Endless(params string[] names)
    {
        for (var i = 0; i < 100; i++)
            yield return names[i % names.Length];
        throw new InvalidOperationException("An endless sequence was read 100 items deep");
    }
}
