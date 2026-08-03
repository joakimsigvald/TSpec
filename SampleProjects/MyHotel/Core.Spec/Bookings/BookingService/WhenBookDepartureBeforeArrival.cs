namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookDepartureBeforeArrival : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookDepartureBeforeArrival()
        => When(_ => _.Book(new BookingRequest(
            A<string>(), ASecond<string>(), new(2026, 8, 12), new(2026, 8, 10))));

    [Fact]
    public void ThenThrowInvalidBookingPeriod()
        => Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
