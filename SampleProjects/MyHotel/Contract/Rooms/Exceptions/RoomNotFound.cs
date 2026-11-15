namespace MyHotel.Contract.Rooms.Exceptions;

public class RoomNotFound(string roomNumber)
    : Exception($"There is no room {roomNumber}.")
{
    public string RoomNumber { get; } = roomNumber;
}
