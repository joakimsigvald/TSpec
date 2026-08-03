using System.Net.Http.Json;
using static System.Net.HttpStatusCode;

namespace MyHotel.Spec.Bookings;

public abstract class WhenBookRoom : ApiSpec<HttpResponseMessage>
{
    protected WhenBookRoom()
        => When(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
            A<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))));

    public class GivenTheRoomExists : WhenBookRoom
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);

        [Fact]
        public async Task ThenReturnTheBookingWithItsAssignedId()
            => (await Result.Read<Booking>()).Is(new Booking(
                1, The<Room>().RoomNumber, The<string>(), new(2026, 8, 10), new(2026, 8, 12)));

        [Fact]
        public void ThenPointAtTheNewBooking()
            => Result.Headers.Location!.ToString().Is("/bookings/1");
    }

    public class GivenNoSuchRoom : WhenBookRoom
    {
        [Fact] public void ThenRespondNotFound() => Result.StatusCode.Is(NotFound);

        [Fact]
        public async Task ThenSayWhichRoom()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Setups run last-declared-first, so the room exists before the earlier guest books it.
    /// </summary>
    public class GivenAnOverlappingBooking : WhenBookRoom
    {
        public GivenAnOverlappingBooking()
            => Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                The<Room>().RoomNumber, ASecond<string>(), new(2026, 8, 11), new(2026, 8, 13))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondConflict() => Result.StatusCode.Is(Conflict);

        [Fact]
        public async Task ThenSayWhichRoom()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Cancelling frees the nights, so the same period books again afterwards. Setups run
    /// last-declared-first: the room exists, the earlier guest books it, that booking is cancelled.
    /// </summary>
    public class GivenTheNightsWereBookedAndCancelled : WhenBookRoom
    {
        public GivenTheNightsWereBookedAndCancelled()
            => Having(_ => _.Api.DeleteAsync("/bookings/1"))
                .Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                    The<Room>().RoomNumber, ASecond<string>(), new(2026, 8, 10), new(2026, 8, 12))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);
    }

    /// <summary>
    /// Nights are half-open: the earlier guest departs on the 10th, so that night is free and the
    /// boundary does not count as overlap.
    /// </summary>
    public class GivenAnAdjacentBooking : WhenBookRoom
    {
        public GivenAnAdjacentBooking()
            => Having(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
                The<Room>().RoomNumber, ASecond<string>(), new(2026, 8, 8), new(2026, 8, 10))))
                .Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondCreated() => Result.StatusCode.Is(Created);
    }
}

public abstract class WhenBookZeroNights : ApiSpec<HttpResponseMessage>
{
    protected WhenBookZeroNights()
        => When(_ => _.Api.PostAsJsonAsync("/bookings", new BookingRequest(
            A<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 10))));

    public class GivenTheRoomExists : WhenBookZeroNights
    {
        public GivenTheRoomExists() => Having(_ => _.Api.PostAsJsonAsync("/rooms", The<Room>()));

        [Fact] public void ThenRespondBadRequest() => Result.StatusCode.Is(BadRequest);

        [Fact]
        public async Task ThenExplainThePeriodMustBePositive()
            => (await Result.Read<ErrorBody>()).Error.Does().Contain("at least one night");
    }
}
