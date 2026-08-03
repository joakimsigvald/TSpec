using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Rooms;

public abstract class WhenDeleteRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenDeleteRoom() => When(_ => _.Api.DeleteAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenDeleteRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichRoom()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain(The<Room>().RoomNumber);
    }
}