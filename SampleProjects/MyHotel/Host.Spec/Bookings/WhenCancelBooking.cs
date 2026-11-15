using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Bookings;

public abstract class WhenCancelBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenCancelBooking() => When(_ => _.Api.DeleteAsync("/bookings/10001"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenCancelBooking
    {
        public GivenTheBookingExists()
            => Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    public class GivenNoSuchBooking : WhenCancelBooking
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichBooking()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain("10001");
    }
}
