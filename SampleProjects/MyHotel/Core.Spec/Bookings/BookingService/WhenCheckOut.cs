using static TSpec.Times;

namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Checking out keeps the booking, marked checked out. Only a checked-in booking can be checked out,
/// and only on its departure day.
/// </summary>
public abstract class WhenCheckOut : Spec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCheckOut() =>
        Using(TodayIs(Today))
            .When(_ => _.CheckOut(The(_booking).BookingNumber));

    public class GivenTheBookingIsCheckedInAndDepartsToday : WhenCheckOut
    {
        public GivenTheBookingIsCheckedInAndDepartsToday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenKeepItMarkedCheckedOut() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCheckedOut(The(_booking)) }));
    }

    public class GivenTheBookingIsCheckedInAndDepartsTomorrow : WhenCheckOut
    {
        public GivenTheBookingIsCheckedInAndDepartsTomorrow()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Tomorrow),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckOut() =>
            Then().Throws<NotDueForCheckOut>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");

        [Fact] public void ThenStoreNothing() => Then<IBookingStore>(nameof(IBookingStore.Save), Never);
    }

    public class GivenTheBookingIsPast : WhenCheckOut
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
        public void ThenThrowNotDueForCheckOut() =>
            Then().Throws<NotDueForCheckOut>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingIsBookedAndDepartsToday : WhenCheckOut
    {
        public GivenTheBookingIsBookedAndDepartsToday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Today),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenThrowNotDueForCheckOut() =>
            Then().Throws<NotDueForCheckOut>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenTheBookingWasCheckedOut : WhenCheckOut
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
        public void ThenThrowNotDueForCheckOut() =>
            Then().Throws<NotDueForCheckOut>()
                .that.Message.Does().Contain($"{The(_booking).BookingNumber}");
    }

    public class GivenNoSuchBooking : WhenCheckOut
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
