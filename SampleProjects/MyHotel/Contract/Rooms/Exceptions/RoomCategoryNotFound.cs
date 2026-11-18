namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomCategoryNotFound(string roomCategory)
    : Exception($"There is no room category {roomCategory}.")
{
    public string RoomCategory { get; } = roomCategory;
}
