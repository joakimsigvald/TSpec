using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookZeroNights : ApiSpec<HttpResponseMessage>
{
    protected WhenBookZeroNights() =>
        Using(new BookingPeriod(Tomorrow, Tomorrow))
            .And(A<BookingInfo>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 1 })
            .When(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));

    public class GivenTheRoomExists : WhenBookZeroNights
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayThePeriodIsInvalid() =>
            (await Result.Read<ErrorBody>()).Code.Is("InvalidBookingPeriod");
    }
}
