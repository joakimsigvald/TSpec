using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec;

public abstract class WhenListRooms : ApiSpec<HttpResponseMessage>
{
    protected WhenListRooms() => When(api => api.GetAsync("/rooms"));

    [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

    public class GivenNoRooms : WhenListRooms
    {
        [Fact]
        public async Task ThenReturnNoRooms() => (await Result.Read<Room[]>()).Is().Empty();
    }

    public class GivenTwoRooms : WhenListRooms
    {
        public GivenTwoRooms()
            => Having(api => api.PostAsJsonAsync("/rooms", ASecond<Room>()))
                .Having(api => api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact]
        public async Task ThenReturnBothRoomsInTheOrderTheyWereCreated()
            => (await Result.Read<Room[]>()).Is().EqualTo(Two<Room>());
    }
}