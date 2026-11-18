using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.RoomCategories;

public abstract class WhenDeleteRoomCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenDeleteRoomCategory() =>
        When(_ => _.Api.DeleteAsync($"/room-categories/{A<RoomCategory>().Name}"));

    public class GivenTheRoomCategoryExists : WhenDeleteRoomCategory
    {
        public GivenTheRoomCategoryExists() =>
            Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    public class GivenNoSuchRoomCategory : WhenDeleteRoomCategory
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoomCategory() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryNotFound");
    }

    /// <summary>
    /// Setups run last-declared-first, so the category exists before a room gets it.
    /// </summary>
    public class GivenARoomHasIt : WhenDeleteRoomCategory
    {
        public GivenARoomHasIt() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/rooms", A<Room>() with { RoomCategory = The<RoomCategory>().Name }))
                .Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomCategoryHasRooms() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryHasRooms");
    }

    /// <summary>
    /// Setups run last-declared-first, so the category is added before Standard is deleted.
    /// </summary>
    public class GivenItIsTheLast : WhenDeleteRoomCategory
    {
        public GivenItIsTheLast() =>
            Having(_ => _.Api.DeleteAsync("/room-categories/Standard"))
                .Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayItIsTheLast() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryIsLast");
    }
}
