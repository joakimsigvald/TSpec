using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenDeleteRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenDeleteRoom() => When(_ => _.Api.DeleteAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenDeleteRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheRoomHasAnOpenBooking : WhenDeleteRoom
    {
        public GivenTheRoomHasAnOpenBooking() =>
            Using(A<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 })
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomIsHeld() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomHeld");
    }
}
