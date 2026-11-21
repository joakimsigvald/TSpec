namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookDepartureBeforeArrival : BookingServiceSpec<Booking>
{
    public WhenBookDepartureBeforeArrival() =>
        Using(new BookingPeriod(Tomorrow, Today))
        .When(_ => _.Book(A<BookingRequest>() with { GuestCount = 1 }));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
