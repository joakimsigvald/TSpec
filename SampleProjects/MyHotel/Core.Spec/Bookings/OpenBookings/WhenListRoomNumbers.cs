namespace MyHotel.Core.Spec.Bookings.OpenBookings;

/// <summary>
/// The guest of the open booking departs today, so it is still current; the others departed
/// yesterday or were cancelled.
/// </summary>
public abstract class WhenListRoomNumbers : Spec<Core.Bookings.OpenBookings, IReadOnlyList<string>>
{
    protected WhenListRoomNumbers() =>
        Using(TodayIs(new(2026, 8, 10)))
            .When(_ => _.RoomNumbers());

    public class GivenOneBookingOfEachStatus : WhenListRoomNumbers
    {
        public GivenOneBookingOfEachStatus() =>
            Given<IBookingStore>().That(_ => _.Load()).Returns(() => [
                A<StoredBooking>() with { To = new(2026, 8, 10), Cancelled = false },
                ASecond<StoredBooking>() with { To = new(2026, 8, 9), Cancelled = false },
                AThird<StoredBooking>() with { To = new(2026, 8, 12), Cancelled = true }]);

        [Fact]
        public void ThenReturnTheRoomOfTheOpenOne() =>
            Result.Is().EqualTo([The<StoredBooking>().RoomNumber]);
    }
}
