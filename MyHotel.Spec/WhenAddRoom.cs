using System.Net;
using System.Net.Http.Json;

namespace MyHotel.Spec;

public abstract class WhenAddRoom : ApiSpec<HttpResponseMessage>
{
    protected static readonly Tag<string> _roomNumber = new(nameof(_roomNumber));

    protected WhenAddRoom()
        => When(api => api.PostAsJsonAsync("/rooms", new Room(The(_roomNumber), 2)));

    public class GivenNoSuchRoom : WhenAddRoom
    {
        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(HttpStatusCode.Created);

        [Fact]
        public async Task ThenReturnTheRoom()
            => (await Result.Read<Room>()).Is(new Room(The(_roomNumber), 2));

        [Fact]
        public void ThenPointAtTheNewRoom()
            => Result.Headers.Location!.ToString().Is($"/rooms/{The(_roomNumber)}");
    }

    public class GivenTheRoomAlreadyExists : WhenAddRoom
    {
        public GivenTheRoomAlreadyExists()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(The(_roomNumber), 3)));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(HttpStatusCode.Conflict);
    }
}
