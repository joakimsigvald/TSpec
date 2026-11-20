using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCheckOut : ApiSpec<HttpResponseMessage>
{
    protected WhenCheckOut() => When(_ => _.Api.PostAsync("/bookings/10001/check-out", null));

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, the guest checks in on the
    /// arrival day, and today moves to the departure day.
    /// </summary>
    public class GivenItsDepartureDay : WhenCheckOut
    {
        public GivenItsDepartureDay()
        {
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
            Having(_ => _.SetToday(new(2026, 8, 12)));
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))));
            Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));
        }

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayItIsNotDueForCheckOut() =>
            (await Result.Read<ErrorBody>()).Code.Is("NotDueForCheckOut");
    }

    public class GivenNoSuchBooking : WhenCheckOut
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchBooking() =>
            (await Result.Read<ErrorBody>()).Code.Is("BookingNotFound");
    }
}
