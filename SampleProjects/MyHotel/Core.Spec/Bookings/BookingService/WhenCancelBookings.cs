namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Only the room's cancellable bookings are cancelled - the open ones, and the no-shows until their
/// departure day has passed: its other bookings are kept as they are, and so are other rooms'.
/// </summary>
public abstract class WhenCancelBookings : BookingServiceSpec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCancelBookings() =>
        Using(TodayIs(Today))
            .And(A<BookingInfo>() with { RoomNumber = A<Room>().RoomNumber })
            .When(_ => _.CancelBookings(A<Room>().RoomNumber));

    public class GivenAnOpenBookingOfTheRoom : WhenCancelBookings
    {
        public GivenAnOpenBookingOfTheRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenMarkItCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenANoShowOfTheRoomDepartingToday : WhenCancelBookings
    {
        public GivenANoShowOfTheRoomDepartingToday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, Today),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenMarkItCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenANoShowOfTheRoomThatDepartedYesterday : WhenCancelBookings
    {
        public GivenANoShowOfTheRoomThatDepartedYesterday()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(EarlierDay, Yesterday),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact] public void ThenKeepItAsItIs() => Then<IBookingStore>(_ => _.Save(new[] { The(_booking) }));
    }

    public class GivenACheckedInBookingOfTheRoom : WhenCancelBookings
    {
        public GivenACheckedInBookingOfTheRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Period = new(Yesterday, Tomorrow),
                Stage = BookingStage.CheckedIn,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact] public void ThenKeepItAsItIs() => Then<IBookingStore>(_ => _.Save(new[] { The(_booking) }));
    }

    public class GivenAnOpenBookingOfAnotherRoom : WhenCancelBookings
    {
        public GivenAnOpenBookingOfAnotherRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                Info = A<BookingInfo>() with { RoomNumber = ASecond<Room>().RoomNumber },
                Period = new(Tomorrow, LaterDay),
                Stage = BookingStage.Booked,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact] public void ThenKeepItAsItIs() => Then<IBookingStore>(_ => _.Save(new[] { The(_booking) }));
    }
}
