namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookZeroGuests : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenBookZeroGuests() =>
        When(_ => _.Book(new BookingRequest(
            A<string>(), ASecond<string>(), 0, new BookingPeriod(Today, LaterDay))));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
