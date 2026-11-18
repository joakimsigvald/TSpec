namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomCategoryIsLast(string roomCategory)
    : Exception($"Room category {roomCategory} is the last one, so it cannot be deleted.")
{
    public string RoomCategory { get; } = roomCategory;
}
