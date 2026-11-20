using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBookThreeGuests : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenBookThreeGuests() =>
        Using(TodayIs(Today))
            .When(_ => _.Book(new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                3,
                new BookingPeriod(LaterDay, LaterDay.AddDays(2)))))
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

        [Fact] public void ThenBookItForThreeGuests() => Result.GuestCount.Is(3);
    }
}
