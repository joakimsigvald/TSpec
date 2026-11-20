using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.RoomCategories;

public abstract class WhenUpdateRoomCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenUpdateRoomCategory() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/room-categories/{A<RoomCategory>().Name}",
            The<RoomCategory>() with { Description = ASecond<string>() }));

    public class GivenTheRoomCategoryExists : WhenUpdateRoomCategory
    {
        public GivenTheRoomCategoryExists() =>
            Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheUpdatedRoomCategory() =>
            (await Result.Read<RoomCategory>())
                .Is(The<RoomCategory>() with { Description = ASecond<string>() });
    }
}
