using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenPayBooking : ApiSpec<HttpResponseMessage>
{
    protected WhenPayBooking() =>
        Using(A<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 })
            .When(_ => _.Api.PostAsync("/bookings/10001/pay", null));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenTheBookingIsOpen : WhenPayBooking
    {
        public GivenTheBookingIsOpen() =>
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, the booking is paid.
    /// </summary>
    public class GivenTheBookingWasAlreadyPaid : WhenPayBooking
    {
        public GivenTheBookingWasAlreadyPaid()
        {
            Having(_ => _.Api.PostAsync("/bookings/10001/pay", null));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayTheBookingIsNotPayable() =>
            (await Result.Read<ErrorBody>()).Code.Is("BookingNotPayable");
    }
}
