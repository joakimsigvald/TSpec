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

    public class GivenNoSuchRoom : WhenDeleteRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoom() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomNotFound");
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheRoomHasAnOpenBooking : WhenDeleteRoom
    {
        public GivenTheRoomHasAnOpenBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomIsHeld() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomHeld");
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, its bookings are cancelled.
    /// </summary>
    public class GivenItsBookingsWereCancelled : WhenDeleteRoom
    {
        public GivenItsBookingsWereCancelled()
        {
            Having(_ => _.Api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}/bookings"));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));
        }

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }
}
