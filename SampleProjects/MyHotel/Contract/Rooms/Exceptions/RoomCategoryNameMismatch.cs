namespace MyHotel.Contract.Rooms.Exceptions;

/// <summary>
/// The name identifies the room category, so an update cannot carry a different one.
/// </summary>
public class RoomCategoryNameMismatch(string roomCategory, string given)
    : Exception($"Room category {roomCategory} cannot be renamed to {given}.")
{
    public string RoomCategory { get; } = roomCategory;
    public string Given { get; } = given;
}
