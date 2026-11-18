namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenList : Spec<Core.Bookings.BookingService, IReadOnlyList<Booking>>
{
    protected WhenList() => Using(TodayIs(new(2026, 8, 10))).When(_ => _.Bookings());

    public class GivenNoBookings : WhenList
    {
        public GivenNoBookings() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

        [Fact] public void ThenReturnNoBookings() => Result.Is().Empty();
    }

    public class GivenTwoBookings : WhenList
    {
        public GivenTwoBookings() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(Two<StoredBooking>);

        [Fact]
        public void ThenReturnThemInTheOrderTheyWereMade() =>
            Result.Select(_ => _.BookingNumber).Is().EqualTo(
                [TheFirst<StoredBooking>().BookingNumber, TheSecond<StoredBooking>().BookingNumber]);
    }
}
