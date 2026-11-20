using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.RoomCategories;

public abstract class WhenListRoomCategories : ApiSpec<HttpResponseMessage>
{
    protected WhenListRoomCategories() => When(_ => _.Api.GetAsync("/room-categories"));

    [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

    public class GivenANewHotel : WhenListRoomCategories
    {
        [Fact]
        public async Task ThenReturnStandard() =>
            (await Result.Read<RoomCategoryOverview[]>())
                .Is().EqualTo([new RoomCategoryOverview("Standard", "Standardrum", false)]);
    }

    /// <summary>
    /// The one case that reads HasRooms set, so it shows the flag reaches the body.
    /// </summary>
    public class GivenARoomHasIt : WhenListRoomCategories
    {
        public GivenARoomHasIt() => Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact]
        public async Task ThenSaySo() =>
            (await Result.Read<RoomCategoryOverview[]>()).Has().OneItem().that.HasRooms.Is(true);
    }

    /// <summary>
    /// Setups run last-declared-first, so the category is added before the application is
    /// restarted.
    /// </summary>
    public class GivenTheApplicationWasRestarted : WhenListRoomCategories
    {
        public GivenTheApplicationWasRestarted() =>
            Having(_ => _.Restart())
                .Having(_ => _.Api.PostAsJsonAsync("/room-categories", A<RoomCategory>()));

        [Fact]
        public async Task ThenReturnStandardAndTheRoomCategory() =>
            (await Result.Read<RoomCategory[]>())
                .Is().EqualTo([new RoomCategory("Standard", "Standardrum"), The<RoomCategory>()]);
    }
}
