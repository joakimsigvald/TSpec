namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenChangeToZeroGuests : Spec<Core.Bookings.BookingService, Booking>
{
    public WhenChangeToZeroGuests() =>
        Using(new BookingPeriod(Today, LaterDay))
            .When(_ => _.Change(Any<int>(), A<BookingRequest>() with { GuestCount = 0 }));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
