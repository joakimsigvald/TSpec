using static MyHotel.Contract.Bookings.BookingAction;
using static MyHotel.Contract.Bookings.BookingStatus;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// A booking's state - its status and what can be done with it today - is worked out from its stage,
/// its dates and today's date, so what the store holds only says what has happened to it. The
/// arrival and departure days still count: a booking turns no-show or past only the day after.
/// </summary>
public abstract class WhenGet : Spec<Core.Bookings.BookingService, Booking>
{
    protected WhenGet() =>
        Using(TodayIs(Today))
            .When(_ => _.BookingFor(A<StoredBooking>().BookingNumber));

    public class GivenItIsBookedAndArrivesToday : WhenGet
    {
        public GivenItIsBookedAndArrivesToday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(Today, LaterDay),
                Stage = BookingStage.Booked,
            }]);

        [Fact]
        public void ThenReturnTheBooking() =>
            Result.BookingNumber.Is(The<StoredBooking>().BookingNumber);

        [Fact]
        public void ThenSayItIsOpenAndCanBeCancelledOrCheckedIn() =>
            Result.State.Is(new BookingState(Open, [Cancel, CheckIn]));
    }

    public class GivenItIsBookedAndArrivesTomorrow : WhenGet
    {
        public GivenItIsBookedAndArrivesTomorrow() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
            }]);

        [Fact]
        public void ThenSayItIsOpenAndCanBeCancelled() => Result.State.Is(new BookingState(Open, [Cancel]));
    }

    public class GivenItIsBookedAndDepartsToday : WhenGet
    {
        public GivenItIsBookedAndDepartsToday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(Yesterday, Today),
                Stage = BookingStage.Booked,
            }]);

        [Fact]
        public void ThenSayItIsANoShowThatCanBeCancelled() =>
            Result.State.Is(new BookingState(NoShow, [Cancel]));
    }

    public class GivenItIsBookedAndDepartedYesterday : WhenGet
    {
        public GivenItIsBookedAndDepartedYesterday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Stage = BookingStage.Booked,
            }]);

        [Fact] public void ThenSayItIsANoShow() => Result.State.Is(new BookingState(NoShow, []));
    }

    public class GivenItIsCheckedInAndDepartsToday : WhenGet
    {
        public GivenItIsCheckedInAndDepartsToday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Stage = BookingStage.CheckedIn,
            }]);

        [Fact]
        public void ThenSayItIsCheckedInAndCanBeCheckedOut() =>
            Result.State.Is(new BookingState(CheckedIn, [CheckOut]));
    }

    public class GivenItIsCheckedInAndDepartsTomorrow : WhenGet
    {
        public GivenItIsCheckedInAndDepartsTomorrow() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(EarlierDay, Tomorrow),
                Stage = BookingStage.CheckedIn,
            }]);

        [Fact] public void ThenSayItIsCheckedIn() => Result.State.Is(new BookingState(CheckedIn, []));
    }

    public class GivenItIsCheckedInAndDepartedYesterday : WhenGet
    {
        public GivenItIsCheckedInAndDepartedYesterday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Stage = BookingStage.CheckedIn,
            }]);

        [Fact] public void ThenSayItIsPast() => Result.State.Is(new BookingState(Past, []));
    }

    public class GivenItWasCheckedOut : WhenGet
    {
        public GivenItWasCheckedOut() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Stage = BookingStage.CheckedOut,
            }]);

        [Fact] public void ThenSayItIsCheckedOut() => Result.State.Is(new BookingState(CheckedOut, []));
    }

    public class GivenItWasCancelled : WhenGet
    {
        public GivenItWasCancelled() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Cancelled,
            }]);

        [Fact] public void ThenSayItIsCancelled() => Result.State.Is(new BookingState(Cancelled, []));
    }

    public class GivenNoSuchBooking : WhenGet
    {
        public GivenNoSuchBooking() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

        [Fact]
        public void ThenThrowBookingNotFound() =>
            Then().Throws<BookingNotFound>()
                .that.Message.Does().Contain($"{The<StoredBooking>().BookingNumber}");
    }
}
