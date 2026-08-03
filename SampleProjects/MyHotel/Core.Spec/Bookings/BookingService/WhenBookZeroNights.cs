namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, so the rule holds
/// whatever the hotel contains.
/// </summary>
public class WhenBookZeroNights : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookZeroNights()
        => When(_ => _.Book(new BookingRequest(
            A<string>(), ASecond<string>(), new(2026, 8, 10), new(2026, 8, 10))));

    [Fact]
    public void ThenThrowInvalidBookingPeriod()
        => Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
