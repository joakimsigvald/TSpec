namespace MyHotel.Contract.Rooms;

/// <summary>
/// A room as the hotel answers with it: the room, and whether it has open bookings, which the
/// hotel works out and a caller never sends.
/// </summary>
public record RoomOverview(string RoomNumber, int BedCount, string RoomCategory, bool HasOpenBookings);
