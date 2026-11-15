using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Rooms;

public abstract class WhenAddRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenAddRoom() => When(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

    public class GivenNoSuchRoom : WhenAddRoom
    {
        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());

        [Fact]
        public void ThenPointAtTheNewRoom()
            => Result.Headers.Location!.ToString().Is($"/rooms/{The<Room>().RoomNumber}");
    }

    public class GivenTheRoomAlreadyExists : WhenAddRoom
    {
        public GivenTheRoomAlreadyExists()
            => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>() with { BedCount = Any<int>() }));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);
    }
}