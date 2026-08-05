using static Moq.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenBook()
        => When(_ => _.Book(new BookingRequest(
            A<Room>().RoomNumber, A<string>(), new(2026, 8, 10), new(2026, 8, 12))));

    public abstract class GivenTheRoomExists : WhenBook
    {
        public GivenTheRoomExists()
            => Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()]);

        public class WithNoBookings : GivenTheRoomExists
        {
            public WithNoBookings()
                => Given<IBookingNumberGenerator>().That(_ => _.Next()).Returns(() => 10001)
                    .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>);

            [Fact]
            public void ThenReturnTheBookingWithTheNumberItWasGiven()
                => Result.Is(new Booking(
                    10001, The<Room>().RoomNumber, The<string>(), new(2026, 8, 10), new(2026, 8, 12)));

            [Fact] public void ThenStoreIt() => Then<IBookingStore>(nameof(IBookingStore.Save), Once);
        }

        /// <summary>
        /// Every booking asks the generator for its own number, so the second gets the second — the
        /// bookings already made have no say in it. The earlier stay is adjacent, so nothing conflicts.
        /// Setups run last-declared-first, so that booking is made before this one.
        /// </summary>
        public class ButIsAlreadyBooked : GivenTheRoomExists
        {
            public ButIsAlreadyBooked()
                => Given<IBookingNumberGenerator>().That(_ => _.Next())
                .First().Returns(() => 10001).AndNext().Returns(() => 10002)
                .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<Booking>)
                .Having(_ => _.Book(new BookingRequest(
                    The<Room>().RoomNumber, ASecond<string>(), new(2026, 8, 8), new(2026, 8, 10))));

            [Fact] public void ThenTakeTheSecondNumber() => Result.BookingNumber.Is(10002);
        }

        public class WithAnOverlappingBooking : GivenTheRoomExists
        {
            public WithAnOverlappingBooking()
                => Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<Booking>() with
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
        public class WithAnAdjacentBooking : GivenTheRoomExists
        {
            public WithAnAdjacentBooking()
                => Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<Booking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    From = new(2026, 8, 8),
                    To = new(2026, 8, 10),
                }]);

            [Fact] public void ThenBookTheRoom() => Result.RoomNumber.Is(The<Room>().RoomNumber);
        }
    }

    public class GivenNoSuchRoom : WhenBook
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowRoomNotFound()
            => Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
