namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookZeroGuests : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookZeroGuests() =>
        When(_ => _.Book(new BookingRequest(
            A<string>(), ASecond<string>(), 0, new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
