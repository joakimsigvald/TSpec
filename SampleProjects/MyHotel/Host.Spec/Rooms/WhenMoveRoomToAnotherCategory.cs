using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenMoveRoomToAnotherCategory : ApiSpec<HttpResponseMessage>
{
    protected WhenMoveRoomToAnotherCategory() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/rooms/{A<Room>().RoomNumber}",
            The<Room>() with { RoomCategory = A<RoomCategory>().Name }));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before the category is added.
    /// </summary>
    public class GivenTheRoomCategoryExists : WhenMoveRoomToAnotherCategory
    {
        public GivenTheRoomCategoryExists() =>
            Having(_ => _.Api.PostAsJsonAsync("/room-categories", The<RoomCategory>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheRoomInIt() =>
            (await Result.Read<Room>()).RoomCategory.Is(The<RoomCategory>().Name);
    }

    public class GivenNoSuchRoomCategory : WhenMoveRoomToAnotherCategory
    {
        public GivenNoSuchRoomCategory() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoomCategory() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomCategoryNotFound");
    }
}
