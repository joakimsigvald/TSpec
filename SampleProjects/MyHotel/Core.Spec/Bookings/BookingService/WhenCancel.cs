using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Cancelling keeps the booking as a record: it is stored again, marked cancelled. Only an open
/// booking, or a no-show until its departure day has passed, can be cancelled.
/// </summary>
public abstract class WhenCancel : BookingServiceSpec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCancel() =>
        Using(TodayIs(Today))
            .When(_ => _.Cancel(The(_booking).BookingNumber));

    public class GivenTheBookingIsOpen : WhenCancel
    {
        public GivenTheBookingIsOpen()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenTheBookingIsOpenAndPaid : WhenCancel
    {
        public GivenTheBookingIsOpenAndPaid()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
                Paid = true,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    /// <summary>
    /// The guest never arrived, but departs today, so the booking can still be cancelled.
    /// </summary>
    public class GivenTheBookingIsANoShowDepartingToday : WhenCancel
    {
        public GivenTheBookingIsANoShowDepartingToday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, Today),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenTheBookingIsANoShowThatDepartedYesterday : WhenCancel
    {
        public GivenTheBookingIsANoShowThatDepartedYesterday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotCancellable() =>
            Then().Throws<BookingNotCancellable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheBookingIsCheckedIn : WhenCancel
    {
        public GivenTheBookingIsCheckedIn()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, Tomorrow),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotCancellable() =>
            Then().Throws<BookingNotCancellable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingWasCancelled : WhenCancel
    {
        public GivenTheBookingWasCancelled()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Cancelled,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotCancellable() =>
            Then().Throws<BookingNotCancellable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenNoSuchBooking : WhenCancel
    {
        public GivenNoSuchBooking() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(Zero<StoredBooking>);

        [Fact]
        public void ThenThrowBookingNotFound() =>
            Then().Throws<BookingNotFound>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }
}
