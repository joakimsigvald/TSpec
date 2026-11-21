using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookThreeGuests : ApiSpec<HttpResponseMessage>
{
    protected WhenBookThreeGuests() =>
        Using(A<BookingInfo>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 3 })
            .When(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));

    public class GivenTheRoomHasTwoBeds : WhenBookThreeGuests
    {
        public GivenTheRoomHasTwoBeds() =>
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>() with { BedCount = 2 }));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayThereAreTooManyGuests() =>
            (await Result.Read<ErrorBody>()).Code.Is("TooManyGuests");
    }
}
