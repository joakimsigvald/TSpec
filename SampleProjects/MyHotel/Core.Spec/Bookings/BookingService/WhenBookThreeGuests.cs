using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBookThreeGuests : BookingServiceSpec<Booking>
{
    protected WhenBookThreeGuests() =>
        Using(A<BookingInfo>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 3 })
        .When(_ => _.Book(A<BookingRequest>()))
        .Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

    public class GivenTheRoomHasTwoBeds : WhenBookThreeGuests
    {
        public GivenTheRoomHasTwoBeds() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>() with { BedCount = 2 }]);

        [Fact]
        public void ThenThrowTooManyGuests() =>
            Then().Throws<TooManyGuests>().that.Message.Does().Contain(The<Room>().RoomNumber);

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheRoomHasThreeBeds : WhenBookThreeGuests
    {
        public GivenTheRoomHasThreeBeds() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>() with { BedCount = 3 }]);

        [Fact] public void ThenBookItForThreeGuests() => Result.Info.GuestCount.Is(3);
    }
}
