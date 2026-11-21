using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public class WhenAddRoomAtZeroKronor : ApiSpec<HttpResponseMessage>
{
    public WhenAddRoomAtZeroKronor() =>
        When(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>() with { PricePerNight = 0 }));

    [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

    [Fact]
    public async Task ThenSayThePricePerNightIsInvalid() =>
        (await Result.Read<ErrorBody>()).Code.Is("InvalidPricePerNight");
}
