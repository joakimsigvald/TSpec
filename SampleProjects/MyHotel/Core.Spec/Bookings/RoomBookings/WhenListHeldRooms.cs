namespace MyHotel.Core.Spec.Bookings.RoomBookings;

/// <summary>
/// A booking holds its room while it is open or checked in, and while it is a no-show that can still
/// be cancelled.
/// </summary>
public abstract class WhenListHeldRooms : Spec<Core.Bookings.RoomBookings, IReadOnlyList<string>>
{
    protected WhenListHeldRooms() =>
        Using(TodayIs(Today))
            .When(_ => _.HeldRooms());

    /// <summary>
    /// An open booking, a checked-in one, and a no-show that departs today.
    /// </summary>
    public class GivenBookingsThatStillHoldTheirRooms : WhenListHeldRooms
    {
        public GivenBookingsThatStillHoldTheirRooms() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(EarlierDay, Today),
                    Stage = BookingStage.CheckedIn,
                },
                AThird<StoredBooking>() with
                {
                    Period = new(Yesterday, Today),
                    Stage = BookingStage.Booked,
                }]);

        [Fact]
        public void ThenReturnTheirRooms() =>
            Result.Is().EqualTo([
                TheFirst<StoredBooking>().Info.RoomNumber,
                TheSecond<StoredBooking>().Info.RoomNumber,
                TheThird<StoredBooking>().Info.RoomNumber]);
    }

    /// <summary>
    /// A past booking, a checked-out one, a cancelled one, and a no-show that departed yesterday.
    /// </summary>
    public class GivenBookingsThatNoLongerHoldTheirRooms : WhenListHeldRooms
    {
        public GivenBookingsThatNoLongerHoldTheirRooms() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(EarlierDay, Yesterday),
                    Stage = BookingStage.CheckedIn,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(EarlierDay, Today),
                    Stage = BookingStage.CheckedOut,
                },
                AThird<StoredBooking>() with
                {
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Cancelled,
                },
                AFourth<StoredBooking>() with
                {
                    Period = new(EarlierDay, Yesterday),
                    Stage = BookingStage.Booked,
                }]);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }
}
