using MyHotel.Core.Rooms;

namespace MyHotel.Core.Bookings;

public sealed class OpenBookings(IBookingStore store, TimeProvider clock) : IOpenBookings
{
    public async Task<IReadOnlyList<string>> RoomNumbers() =>
        [.. (await store.Load()).Where(IsOpenToday).Select(booking => booking.RoomNumber)];

    private bool IsOpenToday(StoredBooking booking) => booking.IsOpenOn(clock.Today());
}
