using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Checking in keeps the booking, marked checked in. Only an open booking can be checked in, and only
/// on its arrival day.
/// </summary>
public abstract class WhenCheckIn : BookingServiceSpec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCheckIn() =>
        Using(TodayIs(Today))
            .When(_ => _.CheckIn(The(_booking).BookingNumber));

    public class GivenTheBookingArrivesToday : WhenCheckIn
    {
        public GivenTheBookingArrivesToday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Today, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCheckedIn() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCheckedIn(The(_booking)) }));
    }

    public class GivenTheBookingArrivesTomorrow : WhenCheckIn
    {
        public GivenTheBookingArrivesTomorrow()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckIn() =>
            Then().Throws<NotDueForCheckIn>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheBookingArrivedYesterday : WhenCheckIn
    {
        public GivenTheBookingArrivedYesterday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckIn() =>
            Then().Throws<NotDueForCheckIn>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingIsCheckedIn : WhenCheckIn
    {
        public GivenTheBookingIsCheckedIn()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Today, LaterDay),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckIn() =>
            Then().Throws<NotDueForCheckIn>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingWasCancelled : WhenCheckIn
    {
        public GivenTheBookingWasCancelled()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Today, LaterDay),
                Stage = BookingStage.Cancelled,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckIn() =>
            Then().Throws<NotDueForCheckIn>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenNoSuchBooking : WhenCheckIn
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
