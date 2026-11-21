namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, so the rule holds
/// whatever the hotel contains.
/// </summary>
public class WhenBookZeroNights : BookingServiceSpec<Booking>
{
    public WhenBookZeroNights() =>
        Using(new BookingPeriod(Tomorrow, Tomorrow))
            .And(A<BookingInfo>() with { GuestCount = 1 })
            .When(_ => _.Book(A<BookingRequest>()));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
