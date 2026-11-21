using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : BookingServiceSpec<Booking>
{
    private static readonly DateOnly Arrival = LaterDay;
    private static readonly DateOnly Departure = LaterDay.AddDays(2);

    protected WhenBook() =>
        Using(TodayIs(Today))
            .And(new BookingPeriod(Arrival, Departure))
            .And(new BookingInfo(A<Room>().RoomNumber, A<string>(), 1))
            .When(_ => _.Book(A<BookingRequest>()));

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
                    The<BookingInfo>(),
                    The<BookingPeriod>(),
                    new BookingState(Open, [Cancel, Pay]),
                    new BookingPayment(2 * The<Room>().PricePerNight, false)));

            [Fact]
            public void ThenStoreItAtThePricePerNightOfItsRoom() =>
                Then<IBookingStore>(_ => _.Save(new[] { new StoredBooking(
                    The<int>(),
                    The<BookingInfo>(),
                    The<BookingPeriod>(),
                    The<Room>().PricePerNight,
                    BookingStage.Booked,
                    false) }));
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
                .Having(_ => _.Book(ASecond<BookingRequest>() with { Period = new(Today, Arrival) }));

            [Fact] public void ThenTakeTheSecondNumber() => Result.BookingNumber.Is(TheSecond<int>());
        }

        public class WithAnOverlappingBooking : GivenTheRoomExists
        {
            public WithAnOverlappingBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    Info = The<BookingInfo>(),
                    Period = new(Arrival.AddDays(1), Departure.AddDays(1)),
                    Stage = BookingStage.Booked,
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
                    Info = The<BookingInfo>(),
                    Period = new(Arrival.AddDays(1), Departure.AddDays(1)),
                    Stage = BookingStage.Cancelled,
                }]);

            [Fact] public void ThenBookTheRoom() => Result.Info.RoomNumber.Is(The<Room>().RoomNumber);
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
                    Info = The<BookingInfo>(),
                    Period = new(Today, Arrival),
                    Stage = BookingStage.Booked,
                }]);

            [Fact] public void ThenBookTheRoom() => Result.Info.RoomNumber.Is(The<Room>().RoomNumber);
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
