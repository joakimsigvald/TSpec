namespace MyHotel.Core.Spec.Bookings.RoomBookings;

/// <summary>
/// A booking can be cancelled while it is open, and while it is a no-show whose departure day has
/// not passed.
/// </summary>
public abstract class WhenListRoomsWithCancellableBookings
    : Spec<Core.Bookings.RoomBookings, IReadOnlyList<string>>
{
    protected WhenListRoomsWithCancellableBookings() =>
        Using(TodayIs(Today))
            .When(_ => _.RoomsWithCancellableBookings());

    /// <summary>
    /// An open booking, and a no-show that departs today.
    /// </summary>
    public class GivenCancellableBookings : WhenListRoomsWithCancellableBookings
    {
        public GivenCancellableBookings() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Booked,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(Yesterday, Today),
                    Stage = BookingStage.Booked,
                }]);

        [Fact]
        public void ThenReturnTheirRooms() =>
            Result.Is().EqualTo([
                TheFirst<StoredBooking>().RoomNumber,
                TheSecond<StoredBooking>().RoomNumber]);
    }

    /// <summary>
    /// A booking checked in, one past and one checked out.
    /// </summary>
    public class GivenBookingsWhoseGuestsCheckedIn : WhenListRoomsWithCancellableBookings
    {
        public GivenBookingsWhoseGuestsCheckedIn() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(EarlierDay, Tomorrow),
                    Stage = BookingStage.CheckedIn,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(EarlierDay, Yesterday),
                    Stage = BookingStage.CheckedIn,
                },
                AThird<StoredBooking>() with
                {
                    Period = new(EarlierDay, Today),
                    Stage = BookingStage.CheckedOut,
                }]);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }

    public class GivenACancelledBookingAndANoShowThatDepartedYesterday : WhenListRoomsWithCancellableBookings
    {
        public GivenACancelledBookingAndANoShowThatDepartedYesterday() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(Tomorrow, LaterDay),
                    Stage = BookingStage.Cancelled,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(EarlierDay, Yesterday),
                    Stage = BookingStage.Booked,
                }]);

        [Fact] public void ThenReturnNoRooms() => Result.Is().Empty();
    }
}
