using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenAddRoomInAnotherCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenAddRoomInAnotherCategory() =>
        When(_ => _.Api.PostAsJsonAsync(
            "/rooms", A<Room>() with { RoomCategory = A<RoomCategory>().Name }));

    public class GivenTheRoomCategoryExists : WhenAddRoomInAnotherCategory
    {
        public GivenTheRoomCategoryExists() =>
            Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact]
        public async Task ThenReturnTheRoomInIt() =>
            (await Result.Read<Room>()).RoomCategory.Is(The<RoomCategory>().Name);
    }

    public class GivenNoSuchRoomCategory : WhenAddRoomInAnotherCategory
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoomCategory() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryNotFound");
    }
}
