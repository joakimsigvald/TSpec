using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public class WhenAddRoomWithTenBeds : ApiSpec<HttpResponseMessage>
{
    public WhenAddRoomWithTenBeds() =>
        When(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>() with { BedCount = 10 }));

    [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

    [Fact]
    public async Task ThenSayTheBedCountIsInvalid() =>
        (await Result.Read<ErrorBody>()).Code.Is("InvalidBedCount");
}
