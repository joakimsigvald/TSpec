namespace MyHotel.Core.Spec.Bookings.OpenBookings;

/// <summary>
/// The guest of the open booking departs today, so it is still current; the others departed
/// yesterday or were cancelled.
/// </summary>
public abstract class WhenListRoomNumbers : Spec<Core.Bookings.OpenBookings, IReadOnlyList<string>>
{
    protected WhenListRoomNumbers() =>
        Using(TodayIs(Today))
            .When(_ => _.RoomNumbers());

    public class GivenOneBookingOfEachStatus : WhenListRoomNumbers
    {
        public GivenOneBookingOfEachStatus() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with
                {
                    Period = new(EarlierDay, Today),
                    Cancelled = false,
                },
                ASecond<StoredBooking>() with
                {
                    Period = new(EarlierDay, Yesterday),
                    Cancelled = false,
                },
                AThird<StoredBooking>() with
                {
                    Period = new(EarlierDay, Tomorrow),
                    Cancelled = true,
                }]);

        [Fact]
        public void ThenReturnTheRoomOfTheOpenOne() =>
            Result.Is().EqualTo([The<StoredBooking>().RoomNumber]);
    }
}
