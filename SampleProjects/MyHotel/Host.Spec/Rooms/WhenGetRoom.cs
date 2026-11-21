using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Rooms;

public abstract class WhenGetRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenGetRoom() => When(_ => _.Api.GetAsync($"/rooms/{A<Room>().RoomNumber}"));

    public class GivenTheRoomExists : WhenGetRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);
        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());
    }

    /// <summary>
    /// The one case that reads the room's flags set, so it shows they reach the body. Setups run
    /// last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheRoomHasAnOpenBooking : WhenGetRoom
    {
        public GivenTheRoomHasAnOpenBooking() =>
            Using(A<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 })
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact]
        public async Task ThenSayItIsHeldWithBookingsToCancel() =>
            (await Result.Read<RoomOverview>()).Is(new RoomOverview(
                The<Room>().RoomNumber, The<Room>().BedCount, The<Room>().RoomCategory, true, true));
    }

    public class GivenNoSuchRoom : WhenGetRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoom() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomNotFound");
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is added before the application is restarted.
    /// Nothing is carried over in memory, so whatever answers afterwards was read back from storage.
    /// </summary>
    public class GivenTheApplicationWasRestarted : WhenGetRoom
    {
        public GivenTheApplicationWasRestarted() =>
            Having(_ => _.Restart())
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);
        [Fact] public async Task ThenReturnTheRoom() => (await Result.Read<Room>()).Is(The<Room>());
    }
}