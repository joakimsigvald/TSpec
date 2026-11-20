using static MyHotel.Contract.Bookings.BookingStatus;
using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : Spec<Core.Bookings.BookingService, Booking>
{
    private static readonly DateOnly Arrival = LaterDay;
    private static readonly DateOnly Departure = LaterDay.AddDays(2);

    protected WhenBook() =>
        Using(TodayIs(Today))
            .When(_ => _.Book(new BookingRequest(
                A<Room>().RoomNumber,
                A<string>(),
                1,
                new BookingPeriod(Arrival, Departure))));

    public abstract class GivenTheRoomExists : WhenBook
    {
        public GivenTheRoomExists() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()]);

        public class WithNoBookings : GivenTheRoomExists
        {
            public WithNoBookings() =>
                Given<IBookingNumberGenerator>().That(_ => _.Next()).Returns(An<int>)
                    .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

            [Fact]
            public void ThenReturnTheBookingWithTheNumberItWasGiven() =>
                Result.Is(new Booking(
                    The<int>(),
                    The<Room>().RoomNumber,
                    The<string>(),
                    1,
                    new BookingPeriod(Arrival, Departure),
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
                .First().Returns(An<int>).AndNext().Returns(ASecond<int>)
                .And<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>)
                .Having(_ => _.Book(new BookingRequest(
                    The<Room>().RoomNumber,
                    ASecond<string>(),
                    1,
                    new BookingPeriod(Today, Arrival))));

            [Fact] public void ThenTakeTheSecondNumber() => Result.BookingNumber.Is(TheSecond<int>());
        }

        public class WithAnOverlappingBooking : GivenTheRoomExists
        {
            public WithAnOverlappingBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(Arrival.AddDays(1), Departure.AddDays(1)),
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
                    Period = new(Arrival.AddDays(1), Departure.AddDays(1)),
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
                    Period = new(Today, Arrival),
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
