using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec;

public abstract class WhenGetRoom : ApiSpec<HttpResponseMessage>
{
    protected static readonly Tag<string> _roomNumber = new();

    protected WhenGetRoom() => When(api => api.GetAsync($"/rooms/{The(_roomNumber)}"));

    public class GivenTheRoomExists : WhenGetRoom
    {
        public GivenTheRoomExists()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(The(_roomNumber), An<int>())));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheRoom()
            => (await Result.Read<Room>()).Is(new Room(The(_roomNumber), The<int>()));
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is created before it is deleted.
    /// </summary>
    public class GivenTheRoomWasDeleted : WhenGetRoom
    {
        public GivenTheRoomWasDeleted()
            => Having(api => api.DeleteAsync($"/rooms/{The(_roomNumber)}"))
                .Having(api => api.PostAsJsonAsync("/rooms", new Room(The(_roomNumber), An<int>())));

        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);
    }
}
