using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenGetBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenGetBooking() =>
        Using(new BookingInfo(The<Room>().RoomNumber, A<string>(), 1))
            .When(_ => _.Api.GetAsync("/bookings/10001"));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenGetBooking
    {
        public GivenTheBookingExists() =>
            Using(new BookingPeriod(Tomorrow, LaterDay))
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                The<BookingInfo>(),
                new BookingPeriod(Tomorrow, LaterDay),
                new BookingState(Open, [Cancel, Pay]),
                new BookingPayment(8 * The<Room>().PricePerNight, false)));
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, the room gets another price.
    /// </summary>
    public class GivenTheRoomGotAnotherPrice : WhenGetBooking
    {
        public GivenTheRoomGotAnotherPrice()
        {
            Having(_ => _.Api.PutAsJsonAsync(
                $"/rooms/{The<Room>().RoomNumber}", The<Room>() with { PricePerNight = ASecond<int>() }));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact]
        public async Task ThenKeepThePriceItWasBookedAt() =>
            (await Result.Read<Booking>()).Payment.Price.Is(8 * The<Room>().PricePerNight);
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
            Using(new BookingPeriod(Tomorrow, LaterDay));
            Having(_ => _.Restart());
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                The<BookingInfo>(),
                new BookingPeriod(Tomorrow, LaterDay),
                new BookingState(Open, [Cancel, Pay]),
                new BookingPayment(8 * The<Room>().PricePerNight, false)));
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
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenSayItIsCancelled() =>
            (await Result.Read<Booking>()).State.Status.Is(Cancelled);
    }

    /// <summary>
    /// The one case that reads back a paid booking, so it shows the store keeps it. Setups run
    /// last-declared-first: the room exists, it is booked, the booking is paid.
    /// </summary>
    public class GivenTheBookingWasPaid : WhenGetBooking
    {
        public GivenTheBookingWasPaid()
        {
            Having(_ => _.Api.PostAsync("/bookings/10001/pay", null));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);
        [Fact] public async Task ThenSayItIsPaid() => (await Result.Read<Booking>()).Payment.Paid.Is(true);
    }
}
