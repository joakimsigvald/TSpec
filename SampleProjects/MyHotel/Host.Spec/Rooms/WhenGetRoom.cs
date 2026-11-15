using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Rooms;

public abstract class WhenGetRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenGetRoom() => When(_ => _.Api.GetAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenGetRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichRoom()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is created before it is deleted.
    /// </summary>
    public class GivenTheRoomWasDeleted : WhenGetRoom
    {
        public GivenTheRoomWasDeleted()
            => Having(_ => _.Api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}"))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichRoom()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is created before it is updated.
    /// </summary>
    public class GivenTheRoomWasUpdated : WhenGetRoom
    {
        public GivenTheRoomWasUpdated()
            => Having(_ => _.Api.PutAsJsonAsync(
                $"/rooms/{The<Room>().RoomNumber}", The<Room>() with { BedCount = ASecond<int>() }))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact]
        public async Task ThenReturnTheUpdatedRoom()
            => (await Result.Read<Room>()).Is(The<Room>() with { BedCount = ASecond<int>() });
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is added before the application is restarted.
    /// Nothing is carried over in memory, so whatever answers afterwards was read back from storage.
    /// </summary>
    public class GivenTheApplicationWasRestarted : WhenGetRoom
    {
        public GivenTheApplicationWasRestarted()
            => Having(_ => _.Restart())
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());
    }
}