using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;
using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// A change keeps the booking's number and what has happened to it, and gives it the room, guest, guest
/// count and period asked for. What can be done with it follows the new dates: it now arrives today.
/// </summary>
public abstract class WhenChange : Spec<Core.Bookings.BookingService, Booking>
{
    private readonly Tag<StoredBooking> _booking = new();
    private readonly Tag<StoredBooking> _otherBooking = new();

    protected WhenChange() =>
        Using(TodayIs(Today))
            .When(_ => _.Change(
                The(_booking).BookingNumber,
                new BookingRequest(
                    A<Room>().RoomNumber,
                    A<string>(),
                    1,
                    new BookingPeriod(Today, LaterDay))));

    public abstract class GivenTheRoomExists : WhenChange
    {
        protected GivenTheRoomExists() =>
            Given<IRoomStore>().That(_ => _.Load()).Returns(() => [The<Room>()]);

        public class WithTheBookingOpen : GivenTheRoomExists
        {
            public WithTheBookingOpen()
            {
                Given(_booking).Is(A<StoredBooking>() with
                {
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
            }

            [Fact]
            public void ThenReturnItChanged() =>
                Result.Is(new Booking(
                    The(_booking).BookingNumber,
                    new BookingInfo(The<Room>().RoomNumber, The<string>(), 1),
                    new BookingPeriod(Today, LaterDay),
                    new BookingState(Open, [Cancel, CheckIn])));

            [Fact]
            public void ThenStoreItChanged() =>
                Then<IBookingStore>(_ => _.Save(new[] { new StoredBooking(
                    The(_booking).BookingNumber,
                    The<Room>().RoomNumber,
                    The<string>(),
                    1,
                    new BookingPeriod(Today, LaterDay),
                    BookingStage.Booked) }));
        }

        public class WithTheBookingCheckedIn : GivenTheRoomExists
        {
            public WithTheBookingCheckedIn()
            {
                Given(_booking).Is(A<StoredBooking>() with
                {
                    Period = new(Yesterday, Tomorrow),
                    Stage = BookingStage.CheckedIn,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
            }

            [Fact] public void ThenKeepItCheckedIn() => Result.State.Is(new BookingState(CheckedIn, []));
        }

        public class WithTheBookingCheckedOut : GivenTheRoomExists
        {
            public WithTheBookingCheckedOut()
            {
                Given(_booking).Is(A<StoredBooking>() with
                {
                    Period = new(EarlierDay, Today),
                    Stage = BookingStage.CheckedOut,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
            }

            [Fact] public void ThenKeepItCheckedOut() => Result.State.Is(new BookingState(CheckedOut, []));
        }

        /// <summary>
        /// A booking does not overlap itself, so it can be moved onto some of its own nights.
        /// </summary>
        public class WithTheBookingHoldingSomeOfTheNights : GivenTheRoomExists
        {
            public WithTheBookingHoldingSomeOfTheNights()
            {
                Given(_booking).Is(A<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
            }

            [Fact] public void ThenChangeIt() => Result.Period.Is(new BookingPeriod(Today, LaterDay));
        }

        public class WithAnotherBookingHoldingSomeOfTheNights : GivenTheRoomExists
        {
            public WithAnotherBookingHoldingSomeOfTheNights()
            {
                Given(_booking).Is(A<StoredBooking>() with { Stage = BookingStage.Booked });
                Given(_otherBooking).Is(ASecond<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking), The(_otherBooking)]);
            }

            [Fact]
            public void ThenThrowRoomAlreadyBooked() =>
                Then().Throws<RoomAlreadyBooked>().that.Message.Does().Contain(The<Room>().RoomNumber);

            [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
        }

        /// <summary>
        /// A cancelled booking holds no nights, so it can be moved onto nights another booking holds.
        /// </summary>
        public class WithTheBookingCancelledAndAnotherHoldingSomeOfTheNights : GivenTheRoomExists
        {
            public WithTheBookingCancelledAndAnotherHoldingSomeOfTheNights()
            {
                Given(_booking).Is(A<StoredBooking>() with { Stage = BookingStage.Cancelled });
                Given(_otherBooking).Is(ASecond<StoredBooking>() with
                {
                    RoomNumber = The<Room>().RoomNumber,
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                });
                Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking), The(_otherBooking)]);
            }

            [Fact] public void ThenMoveIt() => Result.Period.Is(new BookingPeriod(Today, LaterDay));
            [Fact] public void ThenKeepItCancelled() => Result.State.Is(new BookingState(Cancelled, []));
        }

        public class WithNoSuchBooking : GivenTheRoomExists
        {
            public WithNoSuchBooking() =>
                Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

            [Fact]
            public void ThenThrowBookingNotFound() =>
                Then().Throws<BookingNotFound>()
                    .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

            [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
        }
    }

    public class GivenNoSuchRoom : WhenChange
    {
        public GivenNoSuchRoom() => Given<IRoomStore>().That(_ => _.Load()).Returns(Zero<Room>);

        [Fact]
        public void ThenThrowRoomNotFound() =>
            Then().Throws<RoomNotFound>().that.Message.Does().Contain(The<Room>().RoomNumber);
    }
}
