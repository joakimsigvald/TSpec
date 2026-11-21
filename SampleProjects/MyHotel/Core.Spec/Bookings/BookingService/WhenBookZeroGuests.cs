namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookZeroGuests : BookingServiceSpec<Booking>
{
    public WhenBookZeroGuests() =>
        When(_ => _.Book(A<BookingRequest>() with { GuestCount = 0 }));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
