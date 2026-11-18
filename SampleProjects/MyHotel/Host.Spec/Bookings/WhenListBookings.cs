using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenListBookings : ApiSpec<HttpResponseMessage>
{
    protected WhenListBookings() => When(_ => _.Api.GetAsync("/bookings"));

    [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

    public class GivenNoBookings : WhenListBookings
    {
        [Fact]
        public async Task ThenReturnNoBookings() => (await Result.Read<Booking[]>()).Is().Empty();
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before either guest books it.
    /// </summary>
    public class GivenTwoBookings : WhenListBookings
    {
        public GivenTwoBookings()
        {
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber, ASecond<string>(), new(2026, 8, 10), new(2026, 8, 12))));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber, A<string>(), new(2026, 8, 14), new(2026, 8, 16))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact]
        public async Task ThenReturnBothBookingsInTheOrderTheyWereMade() =>
            (await Result.Read<Booking[]>()).Is().EqualTo([
                new Booking(
                    10001,
                    The<Room>().RoomNumber,
                    The<string>(),
                    new(2026, 8, 14),
                    new(2026, 8, 16),
                    Open),
                new Booking(
                    10002,
                    The<Room>().RoomNumber,
                    TheSecond<string>(),
                    new(2026, 8, 10),
                    new(2026, 8, 12),
                    Open)]);
    }
}
