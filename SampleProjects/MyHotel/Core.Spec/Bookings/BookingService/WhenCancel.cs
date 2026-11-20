using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Cancelling keeps the booking as a record: it is stored again, marked cancelled. Only an open
/// booking can be cancelled.
/// </summary>
public abstract class WhenCancel : Spec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCancel() =>
        Using(TodayIs(Today))
            .When(_ => _.Cancel(The(_booking).BookingNumber));

    /// <summary>
    /// The guest departs today, so the booking is still current.
    /// </summary>
    public class GivenTheBookingIsOpen : WhenCancel
    {
        public GivenTheBookingIsOpen()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Cancelled = false,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenTheBookingIsPast : WhenCancel
    {
        public GivenTheBookingIsPast()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Cancelled = false,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotOpen() =>
            Then().Throws<BookingNotOpen>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheBookingWasCancelled : WhenCancel
    {
        public GivenTheBookingWasCancelled()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Tomorrow),
                Cancelled = true,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotOpen() =>
            Then().Throws<BookingNotOpen>()
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
