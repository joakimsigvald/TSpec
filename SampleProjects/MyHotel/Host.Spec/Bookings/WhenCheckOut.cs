using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCheckOut : ApiSpec<HttpResponseMessage>
{
    protected WhenCheckOut() =>
        Using(A<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber, GuestCount = 1 })
            .When(_ => _.Api.PostAsync("/bookings/10001/check-out", null));

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, the guest checks in on the
    /// arrival day, and today moves to the departure day.
    /// </summary>
    public class GivenItsDepartureDay : WhenCheckOut
    {
        public GivenItsDepartureDay()
        {
            Using(new BookingPeriod(Today, Tomorrow));
            Having(_ => _.SetToday(Tomorrow));
            Having(_ => _.Api.PostAsync("/bookings/10001/check-in", null));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondNoContent() => Result.StatusCode.Is(NoContent);
    }

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, and today moves to the
    /// departure day without the guest having checked in.
    /// </summary>
    public class GivenItWasNotCheckedIn : WhenCheckOut
    {
        public GivenItWasNotCheckedIn()
        {
            Using(new BookingPeriod(Today, Tomorrow));
            Having(_ => _.SetToday(Tomorrow));
            Having(_ => _.Api.PostAsJsonAsync("/bookings", A<BookingRequest>()));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayItIsNotDueForCheckOut() =>
            (await Result.Read<ErrorBody>()).Code.Is("NotDueForCheckOut");
    }
}
