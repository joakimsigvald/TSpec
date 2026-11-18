namespace MyHotel.Core.Spec.Bookings.BookingService;

/// <summary>
/// Only the room's open bookings are cancelled: its past and cancelled ones are kept as they are,
/// and so are other rooms' bookings.
/// </summary>
public abstract class WhenCancelOpenBookings : Spec<Core.Bookings.BookingService>
{
    private readonly Tag<StoredBooking> _booking = new();

    protected WhenCancelOpenBookings() =>
        Using(TodayIs(new(2026, 8, 10)))
            .When(_ => _.CancelOpenBookings(A<Room>().RoomNumber));

    /// <summary>
    /// The guest departs today, so the booking is still current.
    /// </summary>
    public class GivenAnOpenBookingOfTheRoom : WhenCancelOpenBookings
    {
        public GivenAnOpenBookingOfTheRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                RoomNumber = The<Room>().RoomNumber,
                To = new(2026, 8, 10),
                Cancelled = false,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact]
        public void ThenMarkItCancelled() =>
            Then<IBookingStore>(_ => _.Save(new[] { MarkedCancelled(The(_booking)) }));
    }

    public class GivenAPastBookingOfTheRoom : WhenCancelOpenBookings
    {
        public GivenAPastBookingOfTheRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                RoomNumber = The<Room>().RoomNumber,
                To = new(2026, 8, 9),
                Cancelled = false,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact] public void ThenKeepItAsItIs() => Then<IBookingStore>(_ => _.Save(new[] { The(_booking) }));
    }

    public class GivenAnOpenBookingOfAnotherRoom : WhenCancelOpenBookings
    {
        public GivenAnOpenBookingOfAnotherRoom()
        {
            Given(_booking).Is(A<StoredBooking>() with
            {
                RoomNumber = ASecond<Room>().RoomNumber,
                To = new(2026, 8, 10),
                Cancelled = false,
            });
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [The(_booking)]);
        }

        [Fact] public void ThenKeepItAsItIs() => Then<IBookingStore>(_ => _.Save(new[] { The(_booking) }));
    }
}
