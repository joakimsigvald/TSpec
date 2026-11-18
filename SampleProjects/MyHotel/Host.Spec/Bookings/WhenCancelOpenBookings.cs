using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCancelOpenBookings : ApiSpec<HttpResponseMessage>
{
    protected WhenCancelOpenBookings() =>
        When(_ => _.Api.DeleteAsync($"/rooms/{A<Room>().RoomNumber}/bookings"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheRoomHasAnOpenBooking : WhenCancelOpenBookings
    {
        public GivenTheRoomHasAnOpenBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }
}
