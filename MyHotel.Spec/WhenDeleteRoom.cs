using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec;

public abstract class WhenDeleteRoom : ApiSpec<HttpResponseMessage>
{
    protected static readonly Tag<string> _roomNumber = new();

    protected WhenDeleteRoom() => When(api => api.DeleteAsync($"/rooms/{The(_roomNumber)}"));

    public class GivenTheRoomExists : WhenDeleteRoom
    {
        public GivenTheRoomExists()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(The(_roomNumber), An<int>())));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);
    }
}
