using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookZeroNights : ApiSpec<HttpResponseMessage>
{
    protected WhenBookZeroNights() =>
        When(_ => _.Api.PostAsJsonAsync(
            "/bookings",
            new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 10)))));

    public class GivenTheRoomExists : WhenBookZeroNights
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayThePeriodIsInvalid() =>
            (await Result.Read<ErrorBody>()).Code.Is("InvalidBookingPeriod");
    }
}
