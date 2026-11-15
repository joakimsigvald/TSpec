using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenRenumberRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenRenumberRoom() =>
        When(_ => _.Api.PutAsJsonAsync(
            $"/rooms/{A<Room>().RoomNumber}", The<Room>() with { RoomNumber = ASecond<string>() }));

    public class GivenTheRoomExists : WhenRenumberRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenSayARoomCannotBeRenumbered() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomNumberMismatch");
    }
}
