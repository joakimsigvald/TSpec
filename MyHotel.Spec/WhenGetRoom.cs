using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec;

public abstract class WhenGetRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenGetRoom() => When(api => api.GetAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenGetRoom
    {
        public GivenTheRoomExists() => Having(api => api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());
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
            => Having(api => api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}"))
                .Having(api => api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is created before it is updated.
    /// </summary>
    public class GivenTheRoomWasUpdated : WhenGetRoom
    {
        public GivenTheRoomWasUpdated()
            => Having(api => api.PutAsJsonAsync(
                $"/rooms/{The<Room>().RoomNumber}", The<Room>() with { BedCount = ASecond<int>() }))
                .Having(api => api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact]
        public async Task ThenReturnTheUpdatedRoom()
            => (await Result.Read<Room>()).Is(The<Room>() with { BedCount = ASecond<int>() });
    }
}