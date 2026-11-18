namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomCategoryHasRooms(string roomCategory)
    : Exception($"Room category {roomCategory} has rooms, so it cannot be deleted.")
{
    public string RoomCategory { get; } = roomCategory;
}
