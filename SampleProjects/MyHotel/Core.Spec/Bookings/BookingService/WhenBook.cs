using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Spec.Bookings.BookingService;

public abstract class WhenBook : BookingServiceSpec<Booking>
{
    private const int Nights = 2;

    private static readonly DateOnly Arrival = LaterDay;
    private static readonly DateOnly Departure = Arrival.AddDays(Nights);

    protected WhenBook() =>
        When(_ => _.Book(A<BookingRequest>(_ => _ with
        {
            Info = new(A<Room>().RoomNumber, A<string>(), A<Room>().BedCount),
            Period = new(Arrival, Departure)
        })));

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
                    The<BookingRequest>().Info,
                    The<BookingRequest>().Period,
                    new BookingState(Open, [Cancel, Pay]),
                    new BookingPayment(Nights * The<Room>().PricePerNight, false)));

            [Fact]
            public void ThenStoreItAtThePricePerNightOfItsRoom() =>
                Then<IBookingStore>(_ => _.Save(new[] { new StoredBooking(
                    The<int>(),
                    The<BookingRequest>().Info,
                    The<BookingRequest>().Period,
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
                .Having(_ => _.Book(ASecond<BookingRequest>() with
                {
                    Info = Any<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber },
                    Period = new(Today, Arrival)
                }));

            [Fact] public void ThenTakeTheSecondNumber() => Result.BookingNumber.Is(TheSecond<int>());
        }

        public class WithAnOverlappingBooking : GivenTheRoomExists
        {
            public WithAnOverlappingBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [A<StoredBooking>() with
                {
                    Info = Any<BookingInfo>() with { RoomNumber = The<Room>().RoomNumber },
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
