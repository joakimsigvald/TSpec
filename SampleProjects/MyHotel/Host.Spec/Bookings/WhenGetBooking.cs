using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenGetBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenGetBooking() => When(_ => _.Api.GetAsync("/bookings/10001"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenGetBooking
    {
        public GivenTheBookingExists() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                The<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)),
                Open));
    }

    public class GivenNoSuchBooking : WhenGetBooking
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchBooking() =>
            (await Result.Read<ErrorBody>()).Code.Is("BookingNotFound");
    }

    /// <summary>
    /// Setups run last-declared-first, so the room is booked before the application is restarted.
    /// Nothing is carried over in memory, so whatever answers afterwards was read back from storage.
    /// </summary>
    public class GivenTheApplicationWasRestarted : WhenGetBooking
    {
        public GivenTheApplicationWasRestarted()
        {
            Having(_ => _.Restart());
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                The<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)),
                Open));
    }

    /// <summary>
    /// Cancelling keeps the booking as a record. Setups run last-declared-first: the room exists,
    /// it is booked, the booking is cancelled.
    /// </summary>
    public class GivenTheBookingWasCancelled : WhenGetBooking
    {
        public GivenTheBookingWasCancelled()
        {
            Having(_ => _.Api.DeleteAsync("/bookings/10001"));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenSayItIsCancelled() => (await Result.Read<Booking>()).Status.Is(Cancelled);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, its open bookings are
    /// cancelled.
    /// </summary>
    public class GivenTheOpenBookingsOfItsRoomWereCancelled : WhenGetBooking
    {
        public GivenTheOpenBookingsOfItsRoomWereCancelled()
        {
            Having(_ => _.Api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}/bookings"));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact]
        public async Task ThenSayItIsCancelled() => (await Result.Read<Booking>()).Status.Is(Cancelled);
    }

    /// <summary>
    /// A deleted room's bookings are kept as a record. Setups run last-declared-first: the room
    /// exists, it is booked, its open bookings are cancelled, it is deleted.
    /// </summary>
    public class GivenItsRoomWasDeleted : WhenGetBooking
    {
        public GivenItsRoomWasDeleted()
        {
            Having(_ => _.Api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}"));
            Having(_ => _.Api.DeleteAsync($"/rooms/{The<Room>().RoomNumber}/bookings"));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenSayItIsCancelled() => (await Result.Read<Booking>()).Status.Is(Cancelled);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, and today moves past the
    /// departure.
    /// </summary>
    public class GivenItsDepartureHasPassed : WhenGetBooking
    {
        public GivenItsDepartureHasPassed()
        {
            Having(_ => _.SetToday(new(2026, 8, 13)));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact]
        public async Task ThenSayItIsPast() => (await Result.Read<Booking>()).Status.Is(Past);
    }
}
