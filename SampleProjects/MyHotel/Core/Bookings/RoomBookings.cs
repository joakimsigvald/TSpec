using MyHotel.Core.Rooms;

namespace MyHotel.Core.Bookings;

public sealed class RoomBookings(IBookingStore store, TimeProvider clock) : IRoomBookings
{
    public async Task<IReadOnlyList<string>> HeldRooms() =>
        [.. (await store.Load()).Where(HoldsItsRoomToday).Select(booking => booking.Info.RoomNumber)];

    public async Task<IReadOnlyList<string>> RoomsWithCancellableBookings() =>
        [.. (await store.Load()).Where(IsCancellableToday).Select(booking => booking.Info.RoomNumber)];

    private bool HoldsItsRoomToday(StoredBooking booking) => booking.HoldsItsRoomOn(clock.Today());
    private bool IsCancellableToday(StoredBooking booking) => booking.IsCancellableOn(clock.Today());
}
