namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, so the rule holds
/// whatever the hotel contains.
/// </summary>
public class WhenBookZeroNights : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookZeroNights() =>
        When(_ => _.Book(new BookingRequest(
            Any<string>(), Any<string>(), 1, new BookingPeriod(Tomorrow, Tomorrow))));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
