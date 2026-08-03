namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenList : Spec<Core.Bookings.BookingService, IReadOnlyList<Booking>>
{
    protected WhenList() => When(_ => _.List());

    public class GivenNoBookings : WhenList
    {
        public GivenNoBookings()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>);

        [Fact] public void ThenReturnNoBookings() => Result.Is().Empty();
    }

    public class GivenTwoBookings : WhenList
    {
        public GivenTwoBookings()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(Two<Booking>);

        [Fact]
        public void ThenReturnThemInTheOrderTheyWereMade() => Result.Is().EqualTo(Two<Booking>());
    }
}
