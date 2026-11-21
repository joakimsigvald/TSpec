namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenChangeToZeroGuests : BookingServiceSpec<Booking>
{
    public WhenChangeToZeroGuests() =>
        When(_ => _.Change(Any<int>(), A<BookingRequest>() with { GuestCount = 0 }));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
