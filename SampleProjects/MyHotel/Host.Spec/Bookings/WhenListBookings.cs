using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
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
                ASecond<BookingRequest>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    GuestCount = 1,
                    Period = new BookingPeriod(Tomorrow, LaterDay),
                }));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                A<BookingRequest>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    GuestCount = 1,
                    Period = new BookingPeriod(LaterDay, LaterDay.AddDays(2)),
                }));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact]
        public async Task ThenReturnBothBookingsInTheOrderTheyWereMade() =>
            (await Result.Read<Booking[]>()).Is().EqualTo([
                new Booking(
                    10001,
                    new BookingInfo(The<Room>().RoomNumber, The<BookingRequest>().GuestName, 1),
                    new BookingPeriod(LaterDay, LaterDay.AddDays(2)),
                    new BookingState(Open, [Cancel])),
                new Booking(
                    10002,
                    new BookingInfo(The<Room>().RoomNumber, TheSecond<BookingRequest>().GuestName, 1),
                    new BookingPeriod(Tomorrow, LaterDay),
                    new BookingState(Open, [Cancel]))]);
    }
}
