using static Moq.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenBook()
        => When(_ => _.Book(new BookingRequest(
            A<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))));

    public class GivenNoBookings : WhenBook
    {
        public GivenNoBookings()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()])
                .Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>);

        [Fact]
        public void ThenReturnTheBookingWithIdOne()
            => Result.Is(new Booking(
                1, The<Room>().RoomNumber, The<string>(), new(2026, 8, 10), new(2026, 8, 12)));

        [Fact] public void ThenStoreIt() => Then<IBookingStore>(nameof(IBookingStore.Save), Once);
    }

    /// <summary>
    /// The earlier booking is for another room, so it cannot conflict — it only shows where the
    /// next id comes from.
    /// </summary>
    public class GivenAnEarlierBooking : WhenBook
    {
        public GivenAnEarlierBooking()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()])
                .Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [A<Booking>() with { Id = 41, RoomNumber = ASecond<string>() }]);

        [Fact] public void ThenAssignTheNextId() => Result.Id.Is(42);
    }

    public class GivenNoSuchRoom : WhenBook
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowRoomNotFound()
            => Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }

    public class GivenAnOverlappingBooking : WhenBook
    {
        public GivenAnOverlappingBooking()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()])
                .Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [A<Booking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    From = new(2026, 8, 11),
                    To = new(2026, 8, 13),
                }]);

        [Fact]
        public void ThenThrowRoomAlreadyBooked()
            => Then().Throws<RoomAlreadyBooked>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }

    /// <summary>
    /// Nights are half-open, [From, To): the earlier guest departs the day this one arrives, so
    /// the boundary night is free and no conflict exists.
    /// </summary>
    public class GivenAnAdjacentBooking : WhenBook
    {
        public GivenAnAdjacentBooking()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()])
                .Given<IBookingStore>().That(_ => _.Load())
                .Returns(() => [A<Booking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    From = new(2026, 8, 8),
                    To = new(2026, 8, 10),
                }]);

        [Fact] public void ThenBookTheRoom() => Result.RoomNumber.Is(The<Room>().RoomNumber);
    }
}
