using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenBookRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenBookRoom() =>
        Using(new BookingPeriod(Tomorrow, LaterDay))
            .When(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                A<BookingRequest>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 1 }));

    public class GivenTheRoomExists : WhenBookRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact]
        public async Task ThenReturnTheBookingWithTheSeededNumber() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                new BookingInfo(The<Room>().RoomNumber, The<BookingRequest>().GuestName, 1),
                new BookingPeriod(Tomorrow, LaterDay),
                new BookingState(Open, [Cancel])));

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
                ASecond<BookingRequest>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    GuestCount = 1,
                    Period = new BookingPeriod(Today, Tomorrow),
                }));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));
        }

        [Fact]
        public async Task ThenCarryOnFromTheNumberAlreadyIssued() =>
            (await Result.Read<Booking>()).BookingNumber.Is(10002);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before the earlier guest books it.
    /// </summary>
    public class GivenAnOverlappingBooking : WhenBookRoom
    {
        public GivenAnOverlappingBooking() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                ASecond<BookingRequest>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    GuestCount = 1,
                    Period = new BookingPeriod(Today, LaterDay),
                }))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheRoomIsAlreadyBooked() =>
            (await Result.Read<ErrorBody>()).Code.Is("RoomAlreadyBooked");
    }
}
