using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCheckIn : ApiSpec<HttpResponseMessage>
{
    protected WhenCheckIn() =>
        Using(A<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 })
            .When(_ => _.Api.PostAsync("/bookings/10001/check-in", null));

    /// <summary>
    /// Setups run last-declared-first, so the room exists before it is booked.
    /// </summary>
    public class GivenItsArrivalDay : WhenCheckIn
    {
        public GivenItsArrivalDay()
        {
            Using(new BookingPeriod(Today, LaterDay));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    /// <summary>
    /// Today is still before the arrival. Setups run last-declared-first, so the room exists before
    /// it is booked.
    /// </summary>
    public class GivenItsArrivalIsLater : WhenCheckIn
    {
        public GivenItsArrivalIsLater() =>
            Using(new BookingPeriod(Tomorrow, LaterDay))
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayItIsNotDueForCheckIn() =>
            (await Result.Read<ErrorBody>()).Code.Is("NotDueForCheckIn");
    }
}
