using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookZeroGuests : ApiSpec<HttpResponseMessage>
{
    protected WhenBookZeroGuests() =>
        When(_ => _.Api.PostAsJsonAsync(
            "/bookings",
            new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                0,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));

    public class GivenTheRoomExists : WhenBookZeroGuests
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayTheGuestCountIsInvalid() =>
            (await Result.Read<ErrorBody>()).Code.Is("InvalidGuestCount");
    }
}
