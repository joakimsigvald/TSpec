namespace MyHotel.Contract.Rooms;

/// <summary>
/// A room category as the hotel answers with it: the category, and whether any room has it, which
/// the hotel works out and a caller never sends.
/// </summary>
public record RoomCategoryOverview(string Name, string Description, bool HasRooms);
