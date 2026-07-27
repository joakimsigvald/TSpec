using System.Net;
using System.Net.Http.Json;

namespace MyHotel.Spec;

public abstract class WhenAddRoom : ApiSpec<HttpResponseMessage>
{
    protected const string RoomNumber = "101";

    protected WhenAddRoom() => When(api => api.PostAsJsonAsync("/rooms", new Room(RoomNumber, 2)));

    public class GivenNoSuchRoom : WhenAddRoom
    {
        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(HttpStatusCode.Created);

        [Fact]
        public async Task ThenReturnTheRoom()
            => (await Result.Read<Room>()).Is(new Room(RoomNumber, 2));

        [Fact]
        public void ThenPointAtTheNewRoom()
            => Result.Headers.Location!.ToString().Is($"/rooms/{RoomNumber}");
    }

    public class GivenTheRoomAlreadyExists : WhenAddRoom
    {
        public GivenTheRoomAlreadyExists()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(RoomNumber, 3)));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(HttpStatusCode.Conflict);
    }
}
