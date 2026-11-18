using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenBookRoom() =>
        When(_ => _.Api.PostAsJsonAsync(
            "/bookings",
            new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));

    public class GivenTheRoomExists : WhenBookRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact]
        public async Task ThenReturnTheBookingWithTheSeededNumber() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                The<Room>().RoomNumber,
                The<string>(),
                1,
                new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)),
                Open));

        [Fact]
        public void ThenPointAtTheNewBooking() =>
            Result.Headers.Location!.ToString().Is("/bookings/10001");
    }

    /// <summary>
    /// The counter is kept with the bookings, not in memory, so it does not start over when the
    /// application does. Setups run last-declared-first: the room exists, a guest books it, the
    /// application restarts.
    /// </summary>
    public class GivenABookingWasMadeBeforeARestart : WhenBookRoom
    {
        public GivenABookingWasMadeBeforeARestart()
        {
            Having(_ => _.Restart());
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 8), new(2026, 8, 10)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));
        }

        [Fact]
        public async Task ThenCarryOnFromTheNumberAlreadyIssued() =>
            (await Result.Read<Booking>()).BookingNumber.Is(10002);
    }

    public class GivenNoSuchRoom : WhenBookRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchRoom() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomNotFound");
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before the earlier guest books it.
    /// </summary>
    public class GivenAnOverlappingBooking : WhenBookRoom
    {
        public GivenAnOverlappingBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 11), new(2026, 8, 13)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomIsAlreadyBooked() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomAlreadyBooked");
    }

    /// <summary>
    /// Cancelling frees the nights, so the same period books again afterwards. Setups run
    /// last-declared-first: the room exists, the earlier guest books it, that booking is cancelled.
    /// </summary>
    public class GivenTheNightsWereBookedAndCancelled : WhenBookRoom
    {
        public GivenTheNightsWereBookedAndCancelled()
        {
            Having(_ => _.Api.DeleteAsync("/bookings/10001"));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));
        }

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        /// The cancelled booking gave its number back to nobody: numbers are issued, not reused.
        [Fact]
        public async Task ThenStillIssueAFreshNumber() =>
            (await Result.Read<Booking>()).BookingNumber.Is(10002);
    }

    /// <summary>
    /// Nights are half-open: the earlier guest departs on the 10th, so that night is free and the
    /// boundary does not count as overlap.
    /// </summary>
    public class GivenAnAdjacentBooking : WhenBookRoom
    {
        public GivenAnAdjacentBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 8), new(2026, 8, 10)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);
    }
}
