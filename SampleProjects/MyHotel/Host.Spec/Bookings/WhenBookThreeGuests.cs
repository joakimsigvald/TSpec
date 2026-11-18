using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookThreeGuests : ApiSpec<HttpResponseMessage>
{
    protected WhenBookThreeGuests() =>
        When(_ => _.Api.PostAsJsonAsync(
            "/bookings",
            new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                3,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));

    public class GivenTheRoomHasTwoBeds : WhenBookThreeGuests
    {
        public GivenTheRoomHasTwoBeds() =>
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>() with { BedCount = 2 }));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayThereAreTooManyGuests() =>
            (await Result.Read<ErrorBody>()).Code.Is("TooManyGuests");
    }
}
