using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Paying keeps the booking, marked paid, all of it at once. Only an open or checked-in booking can be
/// paid, and only once.
/// </summary>
public abstract class WhenPay : BookingServiceSpec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenPay() => When(_ => _.Pay(The(_booking).BookingNumber));

    public class GivenTheBookingIsOpen : WhenPay
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
        public void ThenKeepItMarkedPaid() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedPaid(The(_booking)) }));
    }

    public class GivenTheBookingIsCheckedIn : WhenPay
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
        public void ThenKeepItMarkedPaid() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedPaid(The(_booking)) }));
    }

    public class GivenTheBookingIsPaid : WhenPay
    {
        public GivenTheBookingIsPaid()
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
        public void ThenThrowBookingNotPayable() =>
            Then().Throws<BookingNotPayable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheBookingIsANoShow : WhenPay
    {
        public GivenTheBookingIsANoShow()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, Today),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotPayable() =>
            Then().Throws<BookingNotPayable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingIsPast : WhenPay
    {
        public GivenTheBookingIsPast()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotPayable() =>
            Then().Throws<BookingNotPayable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingWasCheckedOut : WhenPay
    {
        public GivenTheBookingWasCheckedOut()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Stage = BookingStage.CheckedOut,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowBookingNotPayable() =>
            Then().Throws<BookingNotPayable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingWasCancelled : WhenPay
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
        public void ThenThrowBookingNotPayable() =>
            Then().Throws<BookingNotPayable>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenNoSuchBooking : WhenPay
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
