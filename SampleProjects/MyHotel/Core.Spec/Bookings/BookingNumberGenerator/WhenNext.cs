using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingNumberGenerator;

/// <summary>
/// Numbers are issued, never reclaimed: what the generator hands out it also records as the last
/// used, so the next one carries on from there whatever became of the booking that took it.
/// </summary>
public abstract class WhenNext : Spec<Core.Bookings.BookingNumberGenerator, int>
{
    protected WhenNext() =>
        Using(new BookingNumberSeed(An<int>()))
            .When(_ => _.Next());

    public class GivenNoNumberWasIssuedYet : WhenNext
    {
        public GivenNoNumberWasIssuedYet() =>
            Given<IBookingNumberStore>().That(_ => _.LoadLastUsed()).Returns(() => null);

        [Fact] public void ThenIssueTheOneAfterTheSeed() => Result.Is(The<int>() + 1);

        [Fact]
        public void ThenRecordItAsTheLastUsed() =>
            Then<IBookingNumberStore>(_ => _.SaveLastUsed(The<int>() + 1), Once);
    }

    public class GivenANumberWasIssued : WhenNext
    {
        public GivenANumberWasIssued() =>
            Given<IBookingNumberStore>().That(_ => _.LoadLastUsed()).Returns(() => ASecond<int>());

        [Fact] public void ThenIssueTheOneAfterIt() => Result.Is(TheSecond<int>() + 1);

        [Fact]
        public void ThenRecordItAsTheLastUsed() =>
            Then<IBookingNumberStore>(_ => _.SaveLastUsed(TheSecond<int>() + 1), Once);
    }
}
