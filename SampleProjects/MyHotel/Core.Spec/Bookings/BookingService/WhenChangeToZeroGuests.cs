namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenChangeToZeroGuests : BookingServiceSpec<Booking>
{
    public WhenChangeToZeroGuests() =>
        Using(A<BookingInfo>() with { GuestCount = 0 })
            .When(_ => _.Change(Any<int>(), A<BookingRequest>()));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
