using MyHotel.Contract;

namespace MyHotel.Core.Bookings;

/// <summary>
/// Where bookings are kept. Declared here and implemented outward, so Core owns the shape of its
/// own storage. Whole-list load and save, like <see cref="Rooms.IRoomStore"/> and for the same
/// reason: the rules live in Core.
/// </summary>
public interface IBookingStore
{
    Task<IReadOnlyList<Booking>> Load();

    Task Save(IReadOnlyList<Booking> bookings);
}
