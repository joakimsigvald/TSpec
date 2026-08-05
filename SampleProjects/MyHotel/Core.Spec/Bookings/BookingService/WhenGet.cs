namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenGet : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenGet() => When(_ => _.Get(A<Booking>().BookingNumber));

    public class GivenTheBookingExists : WhenGet
    {
        public GivenTheBookingExists()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(One<Booking>);

        [Fact] public void ThenReturnTheBooking() => Result.Is(The<Booking>());
    }

    public class GivenNoSuchBooking : WhenGet
    {
        public GivenNoSuchBooking()
            => Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>);

        [Fact]
        public void ThenThrowBookingNotFound()
            => Then().Throws<BookingNotFound>()
                .that.Message.Does().Contain($"{The<Booking>().BookingNumber}");
    }
}
