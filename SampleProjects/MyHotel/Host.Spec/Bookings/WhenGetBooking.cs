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
        public async Task ThenSayItIsCancelled() =>
            (await Result.Read<Booking>()).State.Status.Is(Cancelled);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, its bookings are cancelled.
    /// </summary>
    public class GivenTheBookingsOfItsRoomWereCancelled : WhenGetBooking
    {
        public GivenTheBookingsOfItsRoomWereCancelled()
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
        public async Task ThenSayItIsCancelled() =>
            (await Result.Read<Booking>()).State.Status.Is(Cancelled);
    }

    /// <summary>
    /// A deleted room's bookings are kept as a record. Setups run last-declared-first: the room
    /// exists, it is booked, its bookings are cancelled, it is deleted.
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
        public async Task ThenSayItIsCancelled() =>
            (await Result.Read<Booking>()).State.Status.Is(Cancelled);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, and today moves past the
    /// arrival.
    /// </summary>
    public class GivenItsArrivalHasPassed : WhenGetBooking
    {
        public GivenItsArrivalHasPassed()
        {
            Having(_ => _.SetToday(new(2026, 8, 11)));
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
        public async Task ThenSayItIsANoShow() => (await Result.Read<Booking>()).State.Status.Is(NoShow);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, today is its arrival day and
    /// the guest checks in.
    /// </summary>
    public class GivenItWasCheckedIn : WhenGetBooking
    {
        public GivenItWasCheckedIn()
        {
            Having(_ => _.Api.PostAsync("/bookings/10001/check-in", null));
            Having(_ => _.SetToday(new(2026, 8, 10)));
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
        public async Task ThenSayItIsCheckedIn() =>
            (await Result.Read<Booking>()).State.Status.Is(CheckedIn);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, the guest checks in on the
    /// arrival day and out on the departure day.
    /// </summary>
    public class GivenItWasCheckedOut : WhenGetBooking
    {
        public GivenItWasCheckedOut()
        {
            Having(_ => _.Api.PostAsync("/bookings/10001/check-out", null));
            Having(_ => _.SetToday(new(2026, 8, 12)));
            Having(_ => _.Api.PostAsync("/bookings/10001/check-in", null));
            Having(_ => _.SetToday(new(2026, 8, 10)));
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
        public async Task ThenSayItIsCheckedOut() =>
            (await Result.Read<Booking>()).State.Status.Is(CheckedOut);
    }
}
