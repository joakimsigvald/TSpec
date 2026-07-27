using System.Net;
using System.Net.Http.Json;

namespace MyHotel.Spec;

public abstract class WhenGetRoom : ApiSpec<HttpResponseMessage>
{
    protected const string RoomNumber = "101";

    protected WhenGetRoom() => When(api => api.GetAsync($"/rooms/{RoomNumber}"));

    public class GivenTheRoomExists : WhenGetRoom
    {
        public GivenTheRoomExists()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(RoomNumber, 2)));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(HttpStatusCode.OK);

        [Fact]
        public async Task ThenReturnTheRoom()
            => (await Result.Read<Room>()).Is(new Room(RoomNumber, 2));
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(HttpStatusCode.NotFound);
    }
}
