namespace MyHotel.Core.Spec.Bookings.BookingService;

public class WhenBookZeroGuests : BookingServiceSpec<Booking>
{
    public WhenBookZeroGuests() =>
        Using(A<BookingInfo>() with { GuestCount = 0 })
            .When(_ => _.Book(A<BookingRequest>()));

    [Fact]
    public void ThenThrowInvalidGuestCount() =>
        Then().Throws<InvalidGuestCount>().that.Message.Does().Contain("at least one guest");
}
