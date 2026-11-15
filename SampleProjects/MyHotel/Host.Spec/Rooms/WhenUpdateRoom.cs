using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenUpdateRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenUpdateRoom() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/rooms/{A<Room>().RoomNumber}", The<Room>() with { BedCount = ASecond<int>() }));

    public class GivenTheRoomExists : WhenUpdateRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheUpdatedRoom() =>
            (await Result.Read<Room>()).Is(The<Room>() with { BedCount = ASecond<int>() });
    }

    public class GivenNoSuchRoom : WhenUpdateRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoom() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomNotFound");
    }
}
