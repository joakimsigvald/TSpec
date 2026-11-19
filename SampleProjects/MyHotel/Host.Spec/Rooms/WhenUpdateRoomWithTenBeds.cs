using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenUpdateRoomWithTenBeds : ApiSpec<HttpResponseMessage>
{
    protected WhenUpdateRoomWithTenBeds() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/rooms/{A<Room>().RoomNumber}", The<Room>() with { BedCount = 10 }));

    public class GivenTheRoomExists : WhenUpdateRoomWithTenBeds
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayTheBedCountIsInvalid() =>
            (await Result.Read<ErrorBody>()).Code.Is("InvalidBedCount");
    }
}
