using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Bookings;

public abstract class WhenGetBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenGetBooking() => When(_ => _.Api.GetAsync("/bookings/1"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenGetBooking
    {
        public GivenTheBookingExists()
            => Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking()
            => (await Result.Read<Booking>()).Is(new Booking(
                1, The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12)));
    }

    public class GivenNoSuchBooking : WhenGetBooking
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichBooking()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain("1");
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is booked before the application is restarted.
    /// Nothing is carried over in memory, so whatever answers afterwards was read back from storage.
    /// </summary>
    public class GivenTheApplicationWasRestarted : WhenGetBooking
    {
        public GivenTheApplicationWasRestarted()
            => Having(_ => _.Restart())
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                    The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking()
            => (await Result.Read<Booking>()).Is(new Booking(
                1, The<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12)));
    }
}
