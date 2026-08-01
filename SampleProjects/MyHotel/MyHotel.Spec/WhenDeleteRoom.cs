using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec;

public abstract class WhenDeleteRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenDeleteRoom() => When(api => api.DeleteAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenDeleteRoom
    {
        public GivenTheRoomExists() => Having(api => api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);
    }
}