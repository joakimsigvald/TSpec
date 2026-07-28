using System.Net;
using System.Net.Http.Json;

namespace MyHotel.Spec;

public abstract class WhenListRooms : ApiSpec<HttpResponseMessage>
{
    protected WhenListRooms() => When(api => api.GetAsync("/rooms"));

    [Fact] public void ThenRespondOk() => Result.StatusCode.Is(HttpStatusCode.OK);

    public class GivenNoRooms : WhenListRooms
    {
        [Fact]
        public async Task ThenReturnNoRooms() => (await Result.Read<Room[]>()).Is().Empty();
    }

    /// <summary>
    /// Setups run last-declared-first, so the room declared last is the one created first.
    /// </summary>
    public class GivenTwoRooms : WhenListRooms
    {
        public GivenTwoRooms()
            => Having(api => api.PostAsJsonAsync("/rooms", new Room(ASecond<string>(), ASecond<int>())))
                .Having(api => api.PostAsJsonAsync("/rooms", new Room(A<string>(), An<int>())));

        [Fact]
        public async Task ThenReturnBothRoomsInTheOrderTheyWereCreated()
        {
            var (first, second) = (await Result.Read<Room[]>()).Has().TwoItems().that;
            first.Is(new Room(The<string>(), The<int>()));
            second.Is(new Room(TheSecond<string>(), TheSecond<int>()));
        }
    }
}
