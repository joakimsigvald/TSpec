using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
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
                new BookingInfo(The<Room>().RoomNumber, A<string>(), 1),
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)),
                new BookingState(Open, [Cancel])));
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
                new BookingInfo(The<Room>().RoomNumber, A<string>(), 1),
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)),
                new BookingState(Open, [Cancel])));
    }

    /// <summary>
    /// The one case that reads back a stage other than booked, so it shows the store keeps it.
    /// Setups run last-declared-first: the room exists, it is booked, the booking is cancelled.
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
        public async Task ThenSayItIsCancelled() =>
            (await Result.Read<Booking>()).State.Status.Is(Cancelled);
    }
}
