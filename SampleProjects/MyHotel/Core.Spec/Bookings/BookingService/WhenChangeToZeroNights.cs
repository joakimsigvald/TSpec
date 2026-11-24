namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// No arrangement at all: the period is refused before any store is consulted, as when booking.
/// </summary>
public class WhenChangeToZeroNights : BookingServiceSpec<Booking>
{
    public WhenChangeToZeroNights() =>
        Using(new BookingPeriod(Tomorrow, Tomorrow))
        .And(A<BookingInfo>() with { GuestCount = 1 })
        .When(_ => _.Change(Any<int>(), A<BookingRequest>()));

    [Fact]
    public void ThenThrowInvalidBookingPeriod() =>
        Then().Throws<InvalidBookingPeriod>().that.Message.Does().Contain("at least one night");
}
