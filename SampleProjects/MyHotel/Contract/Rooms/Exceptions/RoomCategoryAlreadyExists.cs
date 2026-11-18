namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomCategoryAlreadyExists(string roomCategory)
    : Exception($"Room category {roomCategory} already exists.")
{
    public string RoomCategory { get; } = roomCategory;
}
