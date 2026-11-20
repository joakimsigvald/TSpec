using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Host.Spec.Bookings;

public abstract class WhenCheckIn : ApiSpec<HttpResponseMessage>
{
    protected WhenCheckIn() => When(_ => _.Api.PostAsync("/bookings/10001/check-in", null));

    /// <summary>
    /// Setups run last-declared-first: the room exists, it is booked, and today moves to the
    /// arrival day.
    /// </summary>
    public class GivenItsArrivalDay : WhenCheckIn
    {
        public GivenItsArrivalDay()
        {
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
    /// Today is still before the arrival. Setups run last-declared-first, so the room exists before
    /// it is booked.
    /// </summary>
    public class GivenItsArrivalIsLater : WhenCheckIn
    {
        public GivenItsArrivalIsLater() =>
            Having(_ => _.Api.PostAsJsonAsync(
                "/bookings",
                new BookingRequest(
                    The<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(new(2026, 8, 10), new(2026, 8, 12)))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", A<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayItIsNotDueForCheckIn() =>
            (await Result.Read<ErrorBody>()).Code.Is("NotDueForCheckIn");
    }

    public class GivenNoSuchBooking : WhenCheckIn
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayThereIsNoSuchBooking() =>
            (await Result.Read<ErrorBody>()).Code.Is("BookingNotFound");
    }
}
