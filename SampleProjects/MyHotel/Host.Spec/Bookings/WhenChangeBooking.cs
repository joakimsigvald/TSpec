using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenChangeBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenChangeBooking() =>
        When(_ => _.Api.PutAsJsonAsync(
            "/bookings/10001",
            new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(new(2026, 8, 14), new(2026, 8, 16)))));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenChangeBooking
    {
        public GivenTheBookingExists() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheChangedBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                new BookingInfo(The<Room>().RoomNumber, The<string>(), 1),
                new BookingPeriod(new(2026, 8, 14), new(2026, 8, 16)),
                new BookingState(Open, [Cancel])));
    }
}
