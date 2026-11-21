using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCancelBookings : ApiSpec<HttpResponseMessage>
{
    protected WhenCancelBookings() =>
        When(_ => _.Api.DeleteAsync($"/rooms/{A<Room>().RoomNumber}/bookings"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheRoomHasAnOpenBooking : WhenCancelBookings
    {
        public GivenTheRoomHasAnOpenBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                A<BookingRequest>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 }))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }
}
