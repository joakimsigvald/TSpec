using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenChangeToThreeGuests : BookingServiceSpec<Booking>
{
    protected WhenChangeToThreeGuests() =>
        When(_ => _.Change(
            Any<int>(),
            A<BookingRequest>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 3 }));

    public class GivenTheRoomHasTwoBeds : WhenChangeToThreeGuests
    {
        public GivenTheRoomHasTwoBeds() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>() with { BedCount = 2 }]);

        [Fact]
        public void ThenThrowTooManyGuests() =>
            Then().Throws<TooManyGuests>().that.Message.Does().Contain(The<Room>().RoomNumber);

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }
}
