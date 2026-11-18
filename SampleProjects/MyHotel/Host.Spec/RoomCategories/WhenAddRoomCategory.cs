using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.RoomCategories;

public abstract class WhenAddRoomCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenAddRoomCategory() =>
        When(_ => _.Api.PostAsJsonAsync("/room-categories", A<RoomCategory>()));

    public class GivenNoSuchRoomCategory : WhenAddRoomCategory
    {
        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact]
        public async Task ThenReturnTheRoomCategory() =>
            (await Result.Read<RoomCategory>()).Is(The<RoomCategory>());
    }

    public class GivenTheRoomCategoryAlreadyExists : WhenAddRoomCategory
    {
        public GivenTheRoomCategoryAlreadyExists() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/room-categories", The<RoomCategory>() with { Description = Any<string>() }));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomCategoryAlreadyExists() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryAlreadyExists");
    }
}
