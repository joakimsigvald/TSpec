using System.Net.Http.Json;
using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenChangeBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenChangeBooking() =>
        Using(new BookingPeriod(Tomorrow, LaterDay))
            .When(_ => _.Api.PutAsJsonAsync(
                "/bookings/10001",
                A<BookingRequest>() with { RoomNumber = A<Room>().RoomNumber, GuestCount = 1 }));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingExists : WhenChangeBooking
    {
        public GivenTheBookingExists() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                ASecond<BookingRequest>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    GuestCount = 1,
                    Period = new BookingPeriod(Today, Tomorrow),
                }))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondOk() => Result.StatusCode.Is(OK);

        [Fact]
        public async Task ThenReturnTheChangedBooking() =>
            (await Result.Read<Booking>()).Is(new Booking(
                10001,
                new BookingInfo(The<Room>().RoomNumber, The<BookingRequest>().GuestName, 1),
                new BookingPeriod(Tomorrow, LaterDay),
                new BookingState(Open, [Cancel])));
    }
}
