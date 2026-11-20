namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, as when booking.
/// </summary>
public class WhenChangeToZeroNights : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenChangeToZeroNights() =>
        Using(new BookingPeriod(Tomorrow, Tomorrow))
            .When(_ => _.Change(Any<int>(), A<BookingRequest>() with { GuestCount = 1 }));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
