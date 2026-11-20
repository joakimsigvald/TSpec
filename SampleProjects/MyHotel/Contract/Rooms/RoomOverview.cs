namespace MyHotel.Contract.Rooms;

/// <summary>
/// A room as the hotel answers with it: the room, whether a booking still holds it and whether it has
/// bookings to cancel, which the hotel works out and a caller never sends.
/// </summary>
public record RoomOverview(
    string RoomNumber,
    int BedCount,
    string RoomCategory,
    bool Held,
    bool HasCancellableBookings);
