namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookDepartureBeforeArrival : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookDepartureBeforeArrival() =>
        When(_ => _.Book(new BookingRequest(
            A<string>(), ASecond<string>(), 1, new BookingPeriod(Tomorrow, Today))));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
