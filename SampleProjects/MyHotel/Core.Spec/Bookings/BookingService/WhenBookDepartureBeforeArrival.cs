namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookDepartureBeforeArrival : BookingServiceSpec<Booking>
{
    public WhenBookDepartureBeforeArrival() =>
        Using(new BookingPeriod(Tomorrow, Today))
            .And(A<BookingInfo>() with { GuestCount = 1 })
            .When(_ => _.Book(A<BookingRequest>()));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
