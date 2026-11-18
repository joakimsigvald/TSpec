using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.RoomCategories;

public abstract class WhenRenameRoomCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenRenameRoomCategory() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/room-categories/{A<RoomCategory>().Name}",
            The<RoomCategory>() with { Name = ASecond<string>() }));

    public class GivenTheRoomCategoryExists : WhenRenameRoomCategory
    {
        public GivenTheRoomCategoryExists() =>
            Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayARoomCategoryCannotBeRenamed() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryNameMismatch");
    }
}
