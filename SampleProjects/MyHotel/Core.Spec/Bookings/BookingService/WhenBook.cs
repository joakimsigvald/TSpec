using static MyHotel.Contract.Bookings.BookingStatus;
using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenBook() =>
        Using(TodayIs(Today))
            .When(_ => _.Book(new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(LaterDay, LaterDay.AddDays(2)))));

    public abstract class GivenTheRoomExists : WhenBook
    {
        public GivenTheRoomExists() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()]);

        public class WithNoBookings : GivenTheRoomExists
        {
            public WithNoBookings() =>
                Given<IBookingNumberGenerator>().That(_ => _.Next()).Returns(() => 10001)
                    .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

            [Fact]
            public void ThenReturnTheBookingWithTheNumberItWasGiven() =>
                Result.Is(new Booking(
                    10001,
                    The<Room>().RoomNumber,
                    The<string>(),
                    1,
                    new BookingPeriod(LaterDay, LaterDay.AddDays(2)),
                    Open));

            [Fact] public void ThenStoreIt() => Then<IBookingStore>(nameof(IBookingStore.Save), Once);
        }

        /// <summary>
        /// Every booking asks the generator for its own number, so the second gets the second - the
        /// bookings already made have no say in it. The earlier stay is adjacent, so nothing conflicts.
        /// Setups run last-declared-first, so that booking is made before this one.
        /// </summary>
        public class ButIsAlreadyBooked : GivenTheRoomExists
        {
            public ButIsAlreadyBooked() =>
                Given<IBookingNumberGenerator>().That(_ => _.Next())
                .First().Returns(() => 10001).AndNext().Returns(() => 10002)
                .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>)
                .Having(_ => _.Book(new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(LaterDay.AddDays(-2), LaterDay))));

            [Fact] public void ThenTakeTheSecondNumber() => Result.BookingNumber.Is(10002);
        }

        public class WithAnOverlappingBooking : GivenTheRoomExists
        {
            public WithAnOverlappingBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(LaterDay.AddDays(1), LaterDay.AddDays(3)),
                    Cancelled = false,
                }]);

            [Fact]
            public void ThenThrowRoomAlreadyBooked() =>
                Then().Throws<RoomAlreadyBooked>().that.Message.Does().Contain(The<Room>().RoomNumber);
        }

        /// <summary>
        /// A cancelled booking holds no nights, so its period can be booked again.
        /// </summary>
        public class WithAnOverlappingBookingThatWasCancelled : GivenTheRoomExists
        {
            public WithAnOverlappingBookingThatWasCancelled() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(LaterDay.AddDays(1), LaterDay.AddDays(3)),
                    Cancelled = true,
                }]);

            [Fact] public void ThenBookTheRoom() => Result.RoomNumber.Is(The<Room>().RoomNumber);
        }

        /// <summary>
        /// Nights are half-open, [From, To): the earlier guest departs the day this one arrives, so
        /// the boundary night is free and no conflict exists.
        /// </summary>
        public class WithAnAdjacentBooking : GivenTheRoomExists
        {
            public WithAnAdjacentBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(LaterDay.AddDays(-2), LaterDay),
                    Cancelled = false,
                }]);

            [Fact] public void ThenBookTheRoom() => Result.RoomNumber.Is(The<Room>().RoomNumber);
        }
    }

    public class GivenNoSuchRoom : WhenBook
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowRoomNotFound() =>
            Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
