namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, so the rule holds
/// whatever the hotel contains.
/// </summary>
public class WhenBookZeroNights : BookingServiceSpec<Booking>
{
    public WhenBookZeroNights() =>
        Using(new BookingPeriod(Tomorrow, Tomorrow))
            .When(_ => _.Book(A<BookingRequest>() with { GuestCount = 1 }));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
